using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IVehiclePromotionService
    {
        Task<VehiclePromotionResponse> CreateAsync(VehiclePromotionRequest request);
        Task<VehiclePromotionResponse> UpdateAsync(int id, VehiclePromotionUpdateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<VehiclePromotionResponse> GetByIdAsync(int id);
        Task<IEnumerable<VehiclePromotionResponse>> GetAllAsync();

        // Hàm mở rộng
        Task<IEnumerable<VehiclePromotionResponse>> GetPromotionsByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<VehiclePromotionResponse>> GetActivePromotionsAsync();
        Task<IEnumerable<VehiclePromotionResponse>> GetExpiredPromotionsAsync();
        Task<IEnumerable<VehiclePromotionResponse>> GetPromotionByAgencyIdAsync(int agencyId);
    }
}
