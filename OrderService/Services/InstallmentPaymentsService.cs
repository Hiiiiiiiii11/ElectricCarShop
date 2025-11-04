using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public class InstallmentPaymentsService : IInstallmentPaymentsService
    {
        private readonly IInstallmentPaymentsRepository _installmentPaymentsRepository;
        private readonly IInstallmentItemsRepository _installmentItemsRepository;
        private readonly IInstallmentPlansRepository _installmentPlansRepository;
        private readonly ITransactionRepository _transactionRepository;

        public InstallmentPaymentsService(
            IInstallmentPaymentsRepository installmentPaymentsRepository,
            IInstallmentItemsRepository installmentItemsRepository,
            ITransactionRepository transactionRepository,
            IInstallmentPlansRepository installmentPlansRepository
            )
        {
            _installmentPaymentsRepository = installmentPaymentsRepository;
            _installmentItemsRepository = installmentItemsRepository;
            _transactionRepository = transactionRepository;
            _installmentPlansRepository = installmentPlansRepository;
        }

        public async Task<InstallmentPaymentResponse> CreateAsync(InstallmentPaymentRequest request)
        {  var installmentPlan = await _installmentPlansRepository.GetByIdAsync(request.InstallmentPlanId);
            if (installmentPlan == null)
            {
                throw new KeyNotFoundException($"Installment plan with ID {request.InstallmentPlanId} not found.");
            }
            var installmentItem = await _installmentItemsRepository.GetByIdAsync(request.InstallmentItemId);
            if (installmentItem == null)
            {
                throw new KeyNotFoundException($"Installment item with ID {request.InstallmentItemId} not found.");
            }

            var payment = new InstallmentPayments
            {
                InstallmentPlanId = request.InstallmentPlanId,
                InstallmentItemId = request.InstallmentItemId,
                AmountPaid = request.AmountPaid,
                PaidDate = request.PaidDate,
                PaymentMethod = request.PaymentMethod ?? "BankTransfer",
                Status = request.Status ?? "Completed",
                Note = request.Note
            };

            await _installmentPaymentsRepository.AddAsync(payment);
            await _installmentPaymentsRepository.SaveChangesAsync();

            // Cập nhật kỳ nếu thanh toán cho kỳ cụ thể
            if (payment.InstallmentItemId.HasValue)
            {
                // 1. Lấy TẤT CẢ payment cho item này (bao gồm cả cái vừa lưu)
                var allPaymentsForItem = await _installmentPaymentsRepository.GetByItemIdAsync(payment.InstallmentItemId.Value);

                // 2. Tính tổng chính xác
                var totalPaidItem = allPaymentsForItem.Sum(p => p.AmountPaid);

                // 3. Lấy item để cập nhật status
                var item = await _installmentItemsRepository.GetByIdAsync(payment.InstallmentItemId.Value);
                if (item != null)
                {
                    item.Status = totalPaidItem >= item.AmountDue
                        ? "Paid"
                        : totalPaidItem > 0
                            ? "Partial" // Thêm trạng thái "Partial" (đã trả 1 phần)
                            : "Pending";

                    _installmentItemsRepository.Update(item);
                    await _installmentItemsRepository.SaveChangesAsync();
                }
            }

            // Tạo transaction
            var transaction = new Transaction
            {
                InstallPaymentId = payment.Id,
                TransactionCode = GenerateTransactionCode(),
                TransactionDate = DateTime.UtcNow,
                Amount = payment.AmountPaid,
                Status = payment.Status
            };
            await _transactionRepository.AddAsync(transaction);
            await _transactionRepository.SaveChangesAsync();


            return MapToResponse(payment);
        }

        private string GenerateTransactionCode()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"TRANS-INSTALL-{timestamp}-{random}";
        }

        // ✅ UPDATE
        public async Task<InstallmentPaymentResponse> UpdatePaymentAsync(int id, UpdateInstallmentPaymentRequest request)
        {
            var payment = await _installmentPaymentsRepository.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");

            if (request.AmountPaid.HasValue)
                payment.AmountPaid = request.AmountPaid.Value;
            if (request.PaidDate.HasValue)
                payment.PaidDate = request.PaidDate.Value;
            if (!string.IsNullOrEmpty(request.Note))
                payment.Note = request.Note;
            if (!string.IsNullOrEmpty(request.Status))
                payment.Status = request.Status;
            if (!string.IsNullOrEmpty(request.PaymentMethod))
                payment.PaymentMethod = request.PaymentMethod;

            _installmentPaymentsRepository.Update(payment);
            await _installmentPaymentsRepository.SaveChangesAsync();

            return MapToResponse(payment);
        }

        public async Task DeleteAsync(int id)
        {
            var payment = await _installmentPaymentsRepository.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");

            _installmentPaymentsRepository.Remove(payment);
            await _installmentPaymentsRepository.SaveChangesAsync();
        }

        public async Task<List<InstallmentPaymentResponse>> GetPaymentsByItemIdAsync(int itemId)
        {
            var items = await _installmentPaymentsRepository.GetByItemIdAsync(itemId);
            return items.Select(MapToResponse).ToList();
        }

        public async Task<List<InstallmentPaymentResponse>> GetPaymentsByPlanIdAsync(int planId)
        {
            var items = await _installmentPaymentsRepository.GetByPlanIdAsync(planId);
            return items.Select(MapToResponse).ToList();
        }

       
        private static InstallmentPaymentResponse MapToResponse(InstallmentPayments p) => new InstallmentPaymentResponse
        {
            Id = p.Id,
            InstallmentPlanId = p.InstallmentPlanId,
            InstallmentItemId = p.InstallmentItemId,
            AmountPaid = p.AmountPaid,
            PaidDate = p.PaidDate,
            PaymentMethod = p.PaymentMethod,
            Note = p.Note,
            Status = p.Status
        };
    }
}
