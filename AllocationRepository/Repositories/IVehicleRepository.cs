using AllocationRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IVehicleRepository : IGenericRepository<Vehicles>
    {
        Task<Vehicles?> GetVehicleWithDetailsAsync(int vehicleId);
        Task<IEnumerable<Vehicles>> GetVehiclesByStatusAsync(string status);
        Task<IEnumerable<Vehicles>> GetVehiclesWithPromotionsAsync();
        Task<IEnumerable<Vehicles>> GetVehiclesWithAvailableStockAsync();
        Task<decimal?> GetCurrentPriceAsync(int vehicleId, DateTime date);
        Task<IEnumerable<Vehicles>> SearchVehiclesAsync(string? variantName, string? color, string? batteryCapacity);
    }
}
