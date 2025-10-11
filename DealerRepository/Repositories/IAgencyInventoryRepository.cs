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
        Task<AgencyInventory?> GetInventoryAsync(int AgencyId, int variantId);
        Task UpdateInventoryQuantityAsync(int AgencyId, int variantId, int quantity);
        Task SetQuantityAsync(int AgencyId, int variantId, int newQuantity);

        // Kiểm tra tồn kho có đủ hay không
        Task<bool> HasSufficientStockAsync(int AgencyId, int variantId, int requiredQuantity);

        // Xóa tồn kho của 1 variant trong Agency
        Task RemoveInventoryItemAsync(int AgencyId, int variantId);
    }
}
