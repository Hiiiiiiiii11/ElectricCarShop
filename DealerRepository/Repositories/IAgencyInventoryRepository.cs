using AgencyRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyRepository.Repositories
{
    public interface IAgencyInventoryRepository : IGenericRepository<AgencyInventory>
    {
        Task<IEnumerable<AgencyInventory>> GetInventoriesByAgencyIdAsync(int AgencyId);
        Task<AgencyInventory?> GetInventoryAsync(int AgencyId, int vehicleInstanceId);
        Task RemoveInventoryItemAsync(int AgencyId, int vehicleInstanceId);
    }
}
