using AllocationRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IVehiclePromotionRepository: IGenericRepository <VehiclePromotions>
    {
        // 🔹 Lấy danh sách khuyến mãi theo VehicleId
        Task<IEnumerable<VehiclePromotions>> GetPromotionsByVehicleIdAsync(int vehicleId);

        // 🔹 Lấy danh sách khuyến mãi đang có hiệu lực (đang diễn ra)
        Task<IEnumerable<VehiclePromotions>> GetActivePromotionsAsync();

        // 🔹 Lấy lịch sử khuyến mãi (đã hết hạn)
        Task<IEnumerable<VehiclePromotions>> GetExpiredPromotionsAsync();
        Task<IEnumerable<VehiclePromotions>> GetPromotionByAgencyIdAsync(int agencyId);

    }
}
