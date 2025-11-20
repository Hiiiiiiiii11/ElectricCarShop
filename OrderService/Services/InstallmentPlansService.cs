using OrderRepository.Model;
using OrderRepository.Model.OrderDTO;
using OrderRepository.Repositories;
using OrderService.Services;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
using static GrpcService.AgencyGrpcService;

namespace OrderAPIService.Services
{
    public class InstallmentPlansService : IInstallmentPlansService
    {
        private readonly IInstallmentPlansRepository _plansRepository;
        private readonly IInstallmentItemsRepository _itemsRepository;
        private readonly IContractRepository _contractRepository;
        private readonly IAgencyGrpcServiceClient _agencyGrpcServiceClient;

        public InstallmentPlansService(
            IInstallmentPlansRepository plansRepository,
            IInstallmentItemsRepository itemsRepository,
            IContractRepository contractRepository,
            IAgencyGrpcServiceClient agencyGrpcServiceClient
            )
        {
            _plansRepository = plansRepository;
            _itemsRepository = itemsRepository;
            _contractRepository = contractRepository;
            _agencyGrpcServiceClient = agencyGrpcServiceClient;
        }

        // 🟢 CREATE
        public async Task<InstallmentPlanResponse> CreateInstallmentPlanAsync(InstallmentPlanRequest request)
        {
            // 1️⃣ Kiểm tra điều kiện hợp lệ: chỉ được 1 loại ID
            if ((request.ContractId == null && request.AgencyContractId == null) ||
                (request.ContractId != null && request.AgencyContractId != null))
            {
                throw new InvalidOperationException("An installment plan must be linked to either ContractId or AgencyContractId, but not both.");
            }

            // 2️⃣ Nếu là contract plan → kiểm tra tồn tại và trùng lặp
            if (request.ContractId != null)
            {
                var contract = await _contractRepository.GetByIdAsync(request.ContractId.Value);
                if (contract == null)
                    throw new KeyNotFoundException($"Contract with ID {request.ContractId} not found.");

                var plans = await _plansRepository.GetAllAsync();
                if (plans.Any(p => p.ContractId == request.ContractId))
                    throw new InvalidOperationException($"An installment plan already exists for ContractId {request.ContractId}.");
            }

            // 3️⃣ Nếu là agency plan → kiểm tra tồn tại và trùng lặp
            if (request.AgencyContractId != null)
            {
                var agencyContract = await _agencyGrpcServiceClient.GetContractByIdAsync(request.AgencyContractId.Value);
                if (agencyContract == null)
                    throw new KeyNotFoundException($"Agency contract with ID {request.AgencyContractId} not found.");

                var plans = await _plansRepository.GetAllAsync();
                //if (plans.Any(p => p.AgencyContractId == request.AgencyContractId))
                //    throw new InvalidOperationException($"An installment plan already exists for AgencyContractId {request.AgencyContractId}.");
            }

            // 4️⃣ Tạo mới
            var newPlan = new InstallmentPlans
            {
                ContractId = request.ContractId,
                AgencyContractId = request.AgencyContractId,
                PrincipalAmount = request.PrincipalAmount,
                DepositAmount = request.DepositAmount,
                InterestRate = request.InterestRate,
                InterestMethod = request.InterestMethod,
                RuleJson = request.RuleJson,
                Note = request.Note,
                Status = "Active",
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            await _plansRepository.AddAsync(newPlan);
            await _plansRepository.SaveChangesAsync();

            if (!string.IsNullOrEmpty(request.RuleJson))
            {
                var rules = System.Text.Json.JsonSerializer.Deserialize<List<InstallmentRule>>(request.RuleJson);
                if (rules != null && rules.Count > 0)
                {
                    var remainingAmount = request.PrincipalAmount - request.DepositAmount;
                    int currentMonth = 1;
                    int installmentNo = 1;

                    foreach (var rule in rules)
                    {
                        decimal groupAmount = remainingAmount * (decimal)rule.Percentage;

                        // Mỗi tháng trong rule tạo 1 item
                        for (int m = 0; m < rule.Months; m++)
                        {
                            decimal amountDue = groupAmount / rule.Months;
                            decimal principalComponent = amountDue; // (chưa tính lãi)
                            decimal interestComponent = 0;

                            if (request.InterestMethod == "flat")
                            {
                                interestComponent = (remainingAmount * request.InterestRate / 100) / 12;
                            }

                            var item = new InstallmentItems
                            {
                                InstallmentPlanId = newPlan.Id,
                                InstallmentNo = installmentNo++,
                                DueDate = DateTime.UtcNow.AddMonths(currentMonth++),
                                Percentage = (decimal)rule.Percentage / rule.Months,
                                AmountDue = amountDue + interestComponent,
                                PrincipalComponent = principalComponent,
                                InterestComponent = interestComponent,
                                FeeComponent = 0,
                                Notes = $"Auto-generated installment {installmentNo - 1}",
                                Status = "Pending"
                            };

                            await _itemsRepository.AddAsync(item);
                        }
                    }

                    await _itemsRepository.SaveChangesAsync();
                }
            }

            var items = await _itemsRepository.GetByPlanIdAsync(newPlan.Id);
            newPlan.Items = items.ToList();
            return MapToResponse(newPlan);
        }



        // 🟣 UPDATE
        public async Task<InstallmentPlanResponse> UpdateInstallmentPlanAsync(int id, InstallmentPlanUpdateRequest request)
        {
            var plan = await _plansRepository.GetByIdAsync(id);
            if (plan == null)
                throw new KeyNotFoundException($"Installment plan with ID {id} not found.");

            // 1️⃣ Xác định giá trị ContractId và AgencyContractId sau khi cập nhật
            var newContractId = request.ContractId ?? plan.ContractId;
            var newAgencyContractId = request.AgencyContractId ?? plan.AgencyContractId;

            // 2️⃣ Kiểm tra logic: chỉ được 1 loại ID
            if ((newContractId == null && newAgencyContractId == null) ||
                (newContractId != null && newAgencyContractId != null))
            {
                throw new InvalidOperationException("An installment plan must be linked to either ContractId or AgencyContractId, but not both.");
            }

            // 3️⃣ Nếu có ContractId → kiểm tra tồn tại + không trùng lặp với plan khác
            if (newContractId != null)
            {
                var contract = await _contractRepository.GetByIdAsync(newContractId.Value);
                if (contract == null)
                    throw new KeyNotFoundException($"Contract with ID {newContractId} not found.");

                var plans = await _plansRepository.GetAllAsync();
                if (plans.Any(p => p.Id != id && p.ContractId == newContractId))
                    throw new InvalidOperationException($"Another installment plan already exists for ContractId {newContractId}.");
            }

            // 4️⃣ Nếu có AgencyContractId → kiểm tra tồn tại + không trùng lặp với plan khác
            if (newAgencyContractId != null)
            {
                var agencyContract = await _agencyGrpcServiceClient.GetContractByIdAsync(newAgencyContractId.Value);
                if (agencyContract == null)
                    throw new KeyNotFoundException($"Agency contract with ID {newAgencyContractId} not found.");

                var plans = await _plansRepository.GetAllAsync();
                if (plans.Any(p => p.Id != id && p.AgencyContractId == newAgencyContractId))
                    throw new InvalidOperationException($"Another installment plan already exists for AgencyContractId {newAgencyContractId}.");
            }

            // 5️⃣ Cập nhật các trường còn lại
            plan.ContractId = newContractId;
            plan.AgencyContractId = newAgencyContractId;

            if (request.PrincipalAmount.HasValue) plan.PrincipalAmount = request.PrincipalAmount.Value;
            if (request.DepositAmount.HasValue) plan.DepositAmount = request.DepositAmount.Value;
            if (request.InterestRate.HasValue) plan.InterestRate = request.InterestRate.Value;
            if (!string.IsNullOrEmpty(request.InterestMethod)) plan.InterestMethod = request.InterestMethod;
            if (!string.IsNullOrEmpty(request.RuleJson)) plan.RuleJson = request.RuleJson;
            if (!string.IsNullOrEmpty(request.Note)) plan.Note = request.Note;
            if (!string.IsNullOrEmpty(request.Status)) plan.Status = request.Status;

            plan.UpdateAt = DateTime.UtcNow;

            _plansRepository.Update(plan);
            await _plansRepository.SaveChangesAsync();

            return MapToResponse(plan);
        }


        // 🟡 GET BY ID
        public async Task<InstallmentPlanResponse?> GetByIdAsync(int id)
        {
            // ❌ DÒNG CŨ:
            // var plan = await _plansRepository.GetByIdAsync(id);

            // ✅ DÒNG MỚI: Dùng phương thức tải đầy đủ
            var plan = await _plansRepository.GetPlanWithDetailsAsync(id);

            if (plan == null)
                throw new KeyNotFoundException($"Installment plan with ID {id} not found.");

            // ❌ KHÔNG CẦN 2 DÒNG NÀY NỮA, vì "plan" đã bao gồm "Items"
            // var items = await _itemsRepository.GetByPlanIdAsync(plan.Id);
            // plan.Items = items.ToList();

            // "plan" bây giờ đã có đủ cả Items và Payments,
            // MapToResponse sẽ tính toán chính xác.
            return MapToResponse(plan);
        }

        // 🟠 GET ALL
        public async Task<IEnumerable<InstallmentPlanResponse>> GetAllAsync()
        {
            // ❌ DÒNG CŨ:
            // var plans = await _plansRepository.GetAllAsync();

            // ✅ DÒNG MỚI:
            var plans = await _plansRepository.GetAllPlansWithDetailsAsync();

            return plans.Select(MapToResponse);
        }

        // 🔴 DELETE
        public async Task DeleteAsync(int id)
        {
            var plan = await _plansRepository.GetByIdAsync(id);
            if (plan == null)
                throw new KeyNotFoundException($"Installment plan with ID {id} not found.");

            _plansRepository.Remove(plan);
            await _plansRepository.SaveChangesAsync();
        }
        public async Task<InstallmentPlanResponse> GetByContractIdAsync(int contractId)
        {
            var contract = await _contractRepository.GetByIdAsync(contractId);
            if (contract == null)
                throw new KeyNotFoundException($"Contract with ID {contractId} not found.");

            // 'plan' là một object (hoặc null)
            var plan = await _plansRepository.GetByContractIdAsync(contractId);

            // Sửa 2: Kiểm tra null cho plan
            if (plan == null)
            {
                throw new KeyNotFoundException($"No installment plan found for Contract ID {contractId}.");
            }

            // Sửa 3: Chỉ cần map 1 object, không dùng .Select()
            return MapToResponse(plan);
        }

        public async Task<InstallmentPlanResponse> GetByAgencyContractIdAsync(int agencyContractId)
        {
            var agencyContract = await _agencyGrpcServiceClient.GetContractByIdAsync(agencyContractId);
            if (agencyContract == null)
                throw new KeyNotFoundException($"Agency contract with ID {agencyContractId} not found.");

            // 'plan' là một object (hoặc null)
            var plan = await _plansRepository.GetByAgencyContractIdAsync(agencyContractId);

            // Sửa 2: Kiểm tra null cho plan
            if (plan == null)
            {
                throw new KeyNotFoundException($"No installment plan found for Agency Contract ID {agencyContractId}.");
            }

            // Sửa 3: Chỉ cần map 1 object
            return MapToResponse(plan);
        }

        // 🧩 Mapping function
        private static InstallmentPlanResponse MapToResponse(InstallmentPlans p)
        {
            var totalPaid = p.Payments?.Sum(x => x.AmountPaid) ?? 0;

            return new InstallmentPlanResponse
            {
                Id = p.Id,
                ContractId = p.ContractId,
                AgencyContractId = p.AgencyContractId,
                PrincipalAmount = p.PrincipalAmount,
                DepositAmount = p.DepositAmount,
                InterestRate = p.InterestRate,
                InterestMethod = p.InterestMethod,
                Status = p.Status,
                RuleJson = p.RuleJson,
                Note = p.Note,
                CreateAt = p.CreateAt,
                UpdateAt = p.UpdateAt,
                TotalPaid = totalPaid,
                Items = p.Items?.Select(MapToResponseItem).ToList(),
                Payments = p.Payments?.Select(MapToResponsePayment).ToList()
            };
        }

        private static InstallmentItemResponse MapToResponseItem(InstallmentItems i)
        {
            var totalPaid = i.Payments?.Sum(p => p.AmountPaid) ?? 0;
            return new InstallmentItemResponse
            {
                Id = i.Id,
                InstallmentPlanId = i.InstallmentPlanId,
                InstallmentNo = i.InstallmentNo,
                DueDate = i.DueDate,
                Percentage = i.Percentage,
                AmountDue = i.AmountDue,
                PrincipalComponent = i.PrincipalComponent,
                InterestComponent = i.InterestComponent,
                FeeComponent = i.FeeComponent,
                AmountPaid = totalPaid,
                AmountRemaining = i.AmountDue - totalPaid,
                PaidDate = i.Payments?.OrderByDescending(p => p.PaidDate).FirstOrDefault()?.PaidDate,
                Status = i.Status,
                Note = i.Notes
            };
        }

        private static InstallmentPaymentResponse MapToResponsePayment(InstallmentPayments p) => new InstallmentPaymentResponse
        {
            Id = p.Id,
            InstallmentPlanId = p.InstallmentPlanId,
            InstallmentItemId = p.InstallmentItemId,
            AmountPaid = p.AmountPaid,
            PaidDate = p.PaidDate,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            Note = p.Note
        };


    }
}
