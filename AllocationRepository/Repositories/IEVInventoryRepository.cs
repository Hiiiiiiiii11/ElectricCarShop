using AllocationRepository.Model;
using Share.ShareRepo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IEVInventoryRepository : IGenericRepository<EVInventory>
    {
        // 🔍 Lấy tồn kho theo VehicleId
        Task<EVInventory?> GetByVehicleIdAsync(int vehicleId);

        // 🔍 Lấy tất cả tồn kho kèm thông tin xe (Include Vehicle)
        Task<IEnumerable<EVInventory>> GetAllWithVehiclesAsync();

        // 📦 Tăng số lượng tồn kho
        Task IncreaseQuantityAsync(int vehicleId, int quantity);

        // 📦 Giảm số lượng tồn kho
        Task DecreaseQuantityAsync(int vehicleId, int quantity);

        // 🚗 Kiểm tra tồn kho có đủ số lượng không
        Task<bool> HasEnoughStockAsync(int vehicleId, int requiredQuantity);

        // 🔍 Lấy tổng số lượng tồn kho toàn hệ thống
        Task<int> GetTotalInventoryCountAsync();
    }
}
