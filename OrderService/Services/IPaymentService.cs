using OrderRepository.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IPaymentService
    {
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
        Task<PaymentResponse?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentResponse>> GetPaymentsByOrderIdAsync(int orderId);
        Task<IEnumerable<PaymentResponse>> GetPaymentsByStatusAsync(string status);
        Task<decimal> GetTotalPaidByOrderAsync(int orderId);
        Task<PaymentResponse> UpdatePaymentAsync(int id, UpdatePaymentRequest request);
        Task<bool> DeletePaymentAsync(int id);
    }
}

