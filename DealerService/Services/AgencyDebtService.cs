using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyDebtService : IAgencyDebtService
    {
        private readonly IAgencyDebtRepository _agencyDebtRepository;

        public AgencyDebtService(IAgencyDebtRepository agencyDebtRepository)
        {
            _agencyDebtRepository = agencyDebtRepository;
        }

        // ➕ Thêm hoặc cập nhật công nợ cho hợp đồng
        public async Task<AgencyDebtResponse> AddDebtAsync(int agencyId, int contractId, AddAgencyDebtRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            await _agencyDebtRepository.AddOrUpdateDebtAsync(agencyId, contractId, request.Amount, request.Notes);

            var updatedDebt = await _agencyDebtRepository
                .GetByAgencyIdAsync(agencyId); // hoặc GetByAgencyContractAsync nếu bạn thêm hàm này
            return MapToResponse(updatedDebt);
        }

        // ❌ Xóa công nợ (toàn bộ Agency)
        public async Task<AgencyDebtResponse> ClearDebtAsync(int agencyId)
        {
            var debt = await _agencyDebtRepository.GetByAgencyIdAsync(agencyId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {agencyId} not found.");

            await _agencyDebtRepository.ClearDebtAsync(agencyId);
            return MapToResponse(debt);
        }

        // ❌ Xóa công nợ theo hợp đồng (bổ sung mới)
        public async Task<AgencyDebtResponse> ClearDebtByContractAsync(int agencyId, int contractId)
        {
            var debts = await _agencyDebtRepository.GetDebtsByContractAsync(contractId);
            var debt = debts.FirstOrDefault(d => d.AgencyId == agencyId);

            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {agencyId}, ContractId {contractId} not found.");

            _agencyDebtRepository.Remove(debt);
            await _agencyDebtRepository.SaveChangesAsync();

            return MapToResponse(debt);
        }

        // 📄 Lấy tất cả công nợ
        public async Task<IEnumerable<AgencyDebtResponse>> GetAllDebtsAsync()
        {
            var debts = await _agencyDebtRepository.GetAllAsync(d => d.Agency, d => d.AgencyContract);
            return debts.Select(MapToResponse);
        }

        // 📄 Lấy công nợ còn dư
        public async Task<IEnumerable<AgencyDebtResponse>> GetAgencysWithRemainingDebtAsync()
        {
            var debts = await _agencyDebtRepository.GetAgencysWithRemainingDebtAsync();
            return debts.Select(MapToResponse);
        }

        // 🔍 Lấy công nợ theo AgencyId
        public async Task<AgencyDebtResponse> GetDebtByAgencyIdAsync(int agencyId)
        {
            var debt = await _agencyDebtRepository.GetByAgencyIdAsync(agencyId);
            if (debt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {agencyId} not found.");

            return MapToResponse(debt);
        }

        // 🔍 Lấy công nợ theo hợp đồng
        public async Task<IEnumerable<AgencyDebtResponse>> GetDebtsByContractAsync(int contractId)
        {
            var debts = await _agencyDebtRepository.GetDebtsByContractAsync(contractId);
            return debts.Select(MapToResponse);
        }

        // 💰 Thanh toán công nợ
        public async Task<AgencyDebtResponse> MakePaymentAsync(int agencyId, int contractId, MakePaymentRequest request)
        {
            if (request.Amount <= 0)
                throw new ArgumentException("Payment amount must be greater than zero.");

            await _agencyDebtRepository.UpdatePaymentAsync(agencyId, contractId, request.Amount);

            var debts = await _agencyDebtRepository.GetDebtsByContractAsync(contractId);
            var updatedDebt = debts.FirstOrDefault(d => d.AgencyId == agencyId);

            if (updatedDebt == null)
                throw new KeyNotFoundException($"Debt record for AgencyId {agencyId}, ContractId {contractId} not found.");

            return MapToResponse(updatedDebt);
        }

        // 📅 Tìm kiếm công nợ theo ngày
        public async Task<IEnumerable<AgencyDebtResponse>> SearchDebtsAsync(DateTime? fromDate, DateTime? toDate)
        {
            var debts = await _agencyDebtRepository.SearchDebtsAsync(fromDate, toDate);
            return debts.Select(MapToResponse);
        }

        // 🧭 Mapper DTO
        private AgencyDebtResponse MapToResponse(AgencyDebts debt)
        {
            if (debt == null)
                return null;

            return new AgencyDebtResponse
            {
                Id = debt.Id,
                AgencyId = debt.AgencyId,
                AgencyContractId = debt.AgencyContractId,
                DebtAmount = debt.DebtAmount,
                PaidAmount = debt.PaidAmount,
                RemainingAmount = debt.RemainingAmount,
                DueDate = debt.DueDate,
                Status = debt.Status,
                Notes = debt.Notes,
                CreateAt = debt.CreateAt,
                UpdateAt = debt.UpdateAt,
                Agency = debt.Agency == null ? null : new AgencyResponseforDebt
                {
                    Id = debt.Agency.Id,
                    AgencyName = debt.Agency.AgencyName,
                    Address = debt.Agency.Address,
                    Phone = debt.Agency.Phone,
                    Email = debt.Agency.Email,
                    Status = debt.Agency.Status
                },
                Contract = debt.AgencyContract == null ? null : new AgencyContractResponseForDebt
                {
                    Id = debt.AgencyContract.Id,
                    ContractNumber = debt.AgencyContract.ContractNumber,
                    ContractDate = debt.AgencyContract.ContractDate,
                    ContractEndDate = debt.AgencyContract.ContractEndDate,
                    Terms = debt.AgencyContract.Terms,
                    Status = debt.AgencyContract.Status
                }
            };
        }

        public async Task<IEnumerable<AgencyDebtResponse>> GetAllDebtsByAgencyIdAsync(int agencyId)
        {
            var debts = await _agencyDebtRepository.GetAllDebtAgencyIdAsync(agencyId);
            return debts.Select(MapToResponse);
        }

        public async Task<IEnumerable<AgencyDebtResponse>> GetAgencysWithRemainingDebtByAgencyIdAsync(int agencyId)
        {
            var debts = await _agencyDebtRepository.GetAgencysWithRemainingDebtByAgencyIdAsync(agencyId);
            return debts.Select(MapToResponse);
        }
    }
}
