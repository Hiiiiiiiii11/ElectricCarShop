using DealerRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerRepository.Repositories
{
    public interface IDealerInventoryRepository : IGenericRepository<DealerInventory>
    {
        Task<IEnumerable<DealerInventory>> GetInventoriesByDealerIdAsync(int dealerId);
        Task<DealerInventory?> GetInventoryAsync(int dealerId, int variantId);
        Task UpdateInventoryQuantityAsync(int dealerId, int variantId, int quantity);
        Task SetQuantityAsync(int dealerId, int variantId, int newQuantity);

        // Kiểm tra tồn kho có đủ hay không
        Task<bool> HasSufficientStockAsync(int dealerId, int variantId, int requiredQuantity);

        // Xóa tồn kho của 1 variant trong dealer
        Task RemoveInventoryItemAsync(int dealerId, int variantId);
    }
}
