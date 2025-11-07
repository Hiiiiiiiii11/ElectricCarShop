using OrderRepository.Model.OrderDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Services
{
    public interface IInstallmentItemsService
    {
        Task<InstallmentItemResponse> CreateAsync(InstallmentItemRequest request);
        Task<IEnumerable<InstallmentItemResponse>> GetByPlanIdAsync(int planId);
        Task<InstallmentItemResponse> GetByIdAsync(int id);
        Task<IEnumerable<InstallmentItemResponse>> GetAllAsync();
        //Task<InstallmentPlanResponse> UpdateAsync(int id, InstallmentPlanUpdateRequest request);
        Task DeleteAsync(int id);
    }
}
