using OrderRepository.Model;
using OrderRepository.Model.Request;
using OrderRepository.Repositories;
using OrderService.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPIService.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;

        public PaymentService(IPaymentRepository paymentRepository)
        {
            _paymentRepository = paymentRepository;
        }

        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
        {
            var payment = new Payments
            {
                OrderId = request.OrderId,
                PaymentDate = request.PaymentDate,
                Prepay = request.Prepay,
                Amount = request.Amount,
                PaymentMethod = request.PaymentMethod,
                Status = request.Status
            };

            await _paymentRepository.AddAsync(payment);
            return MapToResponse(payment);
        }

        public async Task<PaymentResponse?> GetPaymentByIdAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            return payment == null ? null : MapToResponse(payment);
        }

        public async Task<IEnumerable<PaymentResponse>> GetPaymentsByOrderIdAsync(int orderId)
        {
            var payments = await _paymentRepository.GetByOrderIdAsync(orderId);
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

            // Giữ giá trị cũ nếu không truyền dữ liệu mới
            payment.PaymentDate = request.PaymentDate ?? payment.PaymentDate;
            payment.Prepay = request.Prepay ?? payment.Prepay;
            payment.Amount = request.Amount ?? payment.Amount;
            payment.PaymentMethod = request.PaymentMethod ?? payment.PaymentMethod;
            payment.Status = request.Status ?? payment.Status;

            _paymentRepository.Update(payment);
            await _paymentRepository.SaveChangesAsync();
            return MapToResponse(payment);
        }


        public async Task<bool> DeletePaymentAsync(int id)
        {
            var payment = await _paymentRepository.GetByIdAsync(id);
            if (payment == null)
                return false;

             _paymentRepository.Remove(payment);
            await _paymentRepository.SaveChangesAsync();
            return true;
        }

        private static PaymentResponse MapToResponse(Payments p) => new PaymentResponse
        {
            Id = p.Id,
            OrderId = p.OrderId,
            PaymentDate = p.PaymentDate,
            Prepay = p.Prepay,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            Status = p.Status
        };
    }
}
