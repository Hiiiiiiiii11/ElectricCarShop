using AllocationRepository.Model;
using Share.ShareRepo;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IEVInventoryRepository : IGenericRepository<EVInventory>
    {
        // 🔍 Lấy tồn kho theo VehicleId
        Task<EVInventory?> GetByVehicleInstanceIdAsync(int vehicleInstanceId);

        // 🔍 Lấy tất cả tồn kho kèm thông tin xe (Include Vehicle)
        Task<IEnumerable<EVInventory>> GetAllWithVehiclesAsync();

        // 📦 Tăng số lượng tồn kho
        //Task IncreaseQuantityAsync(int vehicleInstanceId, int quantity);

        //// 📦 Giảm số lượng tồn kho
        //Task DecreaseQuantityAsync(int vehicleInstanceId, int quantity);

        // 🚗 Kiểm tra tồn kho có đủ số lượng không
        //Task<bool> HasEnoughStockAsync(int vehicleInstanceId, int requiredQuantity);

        //// 🔍 Lấy tổng số lượng tồn kho toàn hệ thống
        //Task<int> GetTotalInventoryCountAsync();
    }
}
