using Microsoft.EntityFrameworkCore;
using OrderRepository.Model;
using OrderRepository.Model.OrderDTO;
using OrderRepository.Repositories;
using OrderService.Services;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IAgencyGrpcServiceClient _agencyGrpcServiceClient;
        private readonly IOrderRepository _orderRepository;
        private readonly ITransactionRepository _transactionRepository;

        public PaymentService(IPaymentRepository paymentRepository, IAgencyGrpcServiceClient agencyGrpcServiceClient, IOrderRepository orderRepository, ITransactionRepository transactionRepository)
        {
            _paymentRepository = paymentRepository;
            _agencyGrpcServiceClient = agencyGrpcServiceClient;
            _orderRepository = orderRepository;
            _transactionRepository = transactionRepository;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
        {
            // 🧩 Kiểm tra logic ràng buộc
            if (request.OrderId.HasValue && request.AgencyOrderId.HasValue)
                throw new Exception("Thanh toán không được chứa cả OrderId và AgencyOrderId cùng lúc.");

            if (!request.OrderId.HasValue && !request.AgencyOrderId.HasValue)
                throw new Exception("Thanh toán phải có OrderId hoặc AgencyOrderId.");

            if (request.OrderId.HasValue)
            {
                var order = await _orderRepository.GetByIdAsync(request.OrderId.Value);
                if (order == null)
                    throw new KeyNotFoundException($"Không tìm thấy đơn hàng khách hàng với ID {request.OrderId.Value}.");
            }
            else if (request.AgencyOrderId.HasValue)
            {
                var agencyOrder = await _agencyGrpcServiceClient.GetAgencyOrderByIdAsync(request.AgencyOrderId.Value);
                if (agencyOrder == null)
                    throw new KeyNotFoundException($"Không tìm thấy đơn hàng đại lý với ID {request.AgencyOrderId.Value}.");
            }

            // 🧩 Tạo mới Payment
            var payment = new Payments
            {
                OrderId = request.OrderId,
                AgencyOrderId = request.AgencyOrderId,
                //PaymentDate = DateTime.UtcNow,
                PaymentDate = DateTime.UtcNow.AddMonths(-1),
                Prepay = request.Prepay,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = request.Status
            };

            await _paymentRepository.AddAsync(payment);
            await _paymentRepository.SaveChangesAsync();

            // 🧩 Logic tạo transaction
            decimal transactionAmount = 0;
            string transactionStatus = "Pending";
            DateTime transactionDate = DateTime.UtcNow.AddMonths(-1);

            if (payment.Status == "Completed")
            {
                // ✅ Thanh toán hoàn tất ngay => tạo transaction toàn bộ Amount
                transactionAmount = payment.Amount;
                transactionStatus = "Success";


            }
            else if (payment.Status == "Failed")
            {
                // Thanh toán thất bại
                transactionAmount = 0;
                transactionStatus = "Fail";
            }
            else if (payment.Prepay > 0)
            {
                // ✅ Nếu chưa hoàn tất mà có Prepay, tạo transaction cho phần Prepay
                transactionAmount = payment.Prepay;
                transactionStatus = "Success";
            }

            if (transactionAmount > 0)
            {
                var transaction = new Transaction
                {
                    PaymentId = payment.Id,
                    TransactionCode = GenerateTransactionCode(),
                    //TransactionDate = DateTime.UtcNow,
                    TransactionDate = transactionDate,
                    Amount = transactionAmount,
                    Status = transactionStatus
                };

                await _transactionRepository.AddAsync(transaction);
                await _transactionRepository.SaveChangesAsync();
            }

            return MapToResponse(payment);
        }


        private string GenerateTransactionCode()
        {
            var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            var random = new Random().Next(1000, 9999);
            return $"TRANS-ORDER-{timestamp}-{random}";
        }
        public async Task<PaymentResponse?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");
            return MapToResponse(payment);
        }
        public async Task<IEnumerable<Payments>> GetPaymentsByAgencyOrderIdAsync(int agencyOrderId)
        {
            var agencyorder = await _agencyGrpcServiceClient.GetAgencyOrderByIdAsync(agencyOrderId);
            if (agencyorder == null)
                throw new KeyNotFoundException($"Agency Order with ID {agencyOrderId} not found.");
            return await _paymentRepository.GetByAgencyOrderIdAsync(agencyOrderId);
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var payments = await _paymentRepository.GetByOrderIdAsync(orderId);
            if (payments == null || !payments.Any())
                throw new KeyNotFoundException($"No payments found for Order ID {orderId}.");
            return payments.Select(MapToResponse);
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsByStatusAsync(string status)
        {
            var payments = await _paymentRepository.GetByStatusAsync(status);
            return payments.Select(MapToResponse);
        }

        public async Task<decimal> GetTotalPaidByOrderAsync(int orderId)
        {
            return await _paymentRepository.GetTotalPaidByOrderAsync(orderId);
        }

        public async Task<PaymentResponse> UpdatePaymentAsync(int id, UpdatePaymentRequest request)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");

            // Giữ giá trị cũ
            var oldStatus = payment.Status;

            payment.Prepay = request.Prepay ?? payment.Prepay;
            payment.Amount = request.Amount ?? payment.Amount;
            payment.PaymentMethod = request.PaymentMethod ?? payment.PaymentMethod;
            payment.Status = request.Status ?? payment.Status;

            _paymentRepository.Update(payment);
            await _paymentRepository.SaveChangesAsync();

            // 🧩 Kiểm tra nếu trạng thái chuyển từ pending -> completed
            if (oldStatus?.ToLower() == "pending" && payment.Status?.ToLower() == "completed")
            {
                decimal remaining = payment.Amount - payment.Prepay;

                if (remaining > 0)
                {
                    var transaction = new Transaction
                    {
                        PaymentId = payment.Id,
                        TransactionCode = GenerateTransactionCode(),
                        //TransactionDate = DateTime.UtcNow,
                        TransactionDate = DateTime.UtcNow.AddMonths(-1),
                        Amount = remaining,
                        Status = "completed"
                    };

                    await _transactionRepository.AddAsync(transaction);
                    await _transactionRepository.SaveChangesAsync();
                }
            }

            return MapToResponse(payment);
        }


        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                throw new KeyNotFoundException($"Payment with ID {id} not found.");

            _paymentRepository.Remove(payment);

            try
            {
                await _paymentRepository.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Không thể xóa phiếu thanh toán vì đang được tham chiếu ở bảng khác.", ex);
            }
        }

        private static PaymentResponse MapToResponse(Payments p) => new PaymentResponse
        {
            Id = p.Id,
            OrderId = p.OrderId ?? 0,
            AgencyOrderId = p.AgencyOrderId ?? 0,
            PaymentDate = p.PaymentDate,
            Prepay = p.Prepay,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status,
            TransactionCode = p.Transactions?.FirstOrDefault()?.TransactionCode
        };

        public async Task<IEnumerable<PaymentResponse?>> GetAllPayment()
        {
            var payments = await _paymentRepository.GetAllAsync();
            return payments.Select(MapToResponse);
        }
        public async Task<IEnumerable<PaymentResponse>> GetCustomerPaymentsByAgencyIdAsync(int agencyId)
        {
            var payments = await _paymentRepository.GetCustomerPaymentsByAgencyIdAsync(agencyId);
            return payments.Select(MapToResponse);
        }
    }
}
