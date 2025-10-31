using AgencyRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public interface IAgencyInventoryService
    {
        Task<IEnumerable<AgencyInventoryResponse>> GetInventoriesByAgencyIdAsync(int AgencyId);
        Task<AgencyInventoryResponse?> GetInventoryAsync(int AgencyId, int variantId);
        Task<AgencyInventoryResponse> CreateAgencyInventoryAsync(int AgencyId, CreateAgencyInventoryRequest request);
        Task<AgencyInventoryResponse> UpdateInventoryAsync(int AgencyId, UpdateAgencyInventoryRequest request);

        Task RemoveInventoryItemAsync(int AgencyId, int vehicleInstanceId);

    }
}
