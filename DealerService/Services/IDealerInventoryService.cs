using DealerRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DealerService.Services
{
    public interface IDealerInventoryService
    {
        Task<IEnumerable<DealerInventoryResponse>> GetInventoriesByDealerIdAsync(int dealerId);
        Task<DealerInventoryResponse?> GetInventoryAsync(int dealerId, int variantId);
        Task<DealerInventoryResponse> CreateDealerInventoryAsync(int dealerId, CreateDealerInventoryRequest request);
        Task UpdateInventoryQuantityAsync(int dealerId, UpdateDealerInventoryRequest request);
        Task<bool> HasSufficientStockAsync(int dealerId, int variantId, int requiredQuantity);
        Task RemoveInventoryItemAsync(int dealerId, int variantId);
        Task SetQuantityAsync(int dealerId, int variantId, int newQuantity);
    }
}
