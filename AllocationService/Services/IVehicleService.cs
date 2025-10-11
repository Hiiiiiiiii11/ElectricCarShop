using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IVehicleService
    {
        Task<IEnumerable<VehicleResponse>> GetAllVehiclesAsync();
        Task<VehicleResponse?> GetVehicleByIdAsync(int id);
        Task<VehicleResponse>  AddVehicleAsync(CreateVehicleRequest request);
        Task UpdateVehicleAsync(int vehicleId, UpdateVehicleRequest request);
        Task DeleteVehicleAsync(int id);
        Task<IEnumerable<VehicleResponse>> SearchVehiclesAsync(string? variantName, string? color, string? batteryCapacity);
        Task<IEnumerable<VehicleResponse>> GetVehiclesByStatusAsync(string status);
        Task<IEnumerable<VehicleResponse>> GetVehiclesWithAvailableStockAsync();
        Task<decimal?> GetCurrentPriceAsync(int vehicleId, DateTime date);
    }
}
