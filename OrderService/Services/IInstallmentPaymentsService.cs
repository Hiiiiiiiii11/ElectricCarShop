using OrderRepository.Model.Request;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IInstallmentPaymentsService
    {
        Task<InstallmentPaymentResponse> CreateAsync(InstallmentPaymentRequest request);
        Task<InstallmentPaymentResponse> UpdatePaymentAsync(int paymentId, UpdateInstallmentPaymentRequest request);
        Task DeleteAsync(int id);
        Task<List<InstallmentPaymentResponse>> GetPaymentsByPlanIdAsync(int planId);
        Task<List<InstallmentPaymentResponse>> GetPaymentsByItemIdAsync(int itemId);
    }
}

