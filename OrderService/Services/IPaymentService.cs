using OrderRepository.Model;
using OrderRepository.Model.OrderDTO;
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
        Task<IEnumerable<PaymentResponse?>> GetAllPayment();
        Task<PaymentResponse?> GetPaymentByIdAsync(int id);
        Task<IEnumerable<PaymentResponse>> GetPaymentsByOrderIdAsync(int orderId);
        Task<IEnumerable<Payments>> GetPaymentsByAgencyOrderIdAsync(int agencyOrderId);
        Task<IEnumerable<PaymentResponse>> GetPaymentsByStatusAsync(string status);
        Task<decimal> GetTotalPaidByOrderAsync(int orderId);
        Task<PaymentResponse> UpdatePaymentAsync(int id, UpdatePaymentRequest request);
        Task<bool> DeletePaymentAsync(int id);
        Task<IEnumerable<PaymentResponse>> GetCustomerPaymentsByAgencyIdAsync(int agencyId);
    }
}

