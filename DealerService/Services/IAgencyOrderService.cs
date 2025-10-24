using AgencyRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public interface IAgencyOrderService
    {
        Task<AgencyOrderResponse> CreateAsync(CreateAgencyOrderRequest request);
        Task<AgencyOrderResponse> UpdateAsync(int id,UpdateAgencyOrderRequest request);
        Task<AgencyOrderResponse> GetByIdAsync(int id);
        Task<IEnumerable<AgencyOrderResponse>> GetAllAsync();
        Task<IEnumerable<AgencyOrderResponse>> GetByAgencyAsync(int agencyId);
        Task<IEnumerable<AgencyOrderResponse>> GetByContractAsync(int contractId);
        Task<bool> DeleteAsync(int id);
    }
}
