using AllocationRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IVehiclePriceService
    {
        Task<VehiclePriceResponse> CreateAsync(VehiclePriceRequest request);
        Task<VehiclePriceResponse> UpdateAsync(int id, VehiclePriceUpdateRequest request);
        Task<bool> DeleteAsync(int id);
        Task<VehiclePriceResponse> GetByIdAsync(int id);
        Task<IEnumerable<VehiclePriceResponse>> GetAllAsync();

        // 3 hàm mở rộng
        Task<IEnumerable<VehiclePriceResponse>> GetPricesByVehicleIdAsync(int vehicleId);
        Task<IEnumerable<VehiclePriceResponse>> GetPriceHistoryAsync(int vehicleId);
        Task<IEnumerable<VehiclePriceResponse>> GetPricesByAgencyAsync(int agencyId);
    }
}
