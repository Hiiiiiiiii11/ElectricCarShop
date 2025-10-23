using AllocationRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IVehiclePriceRepository : IGenericRepository<VehiclePrices>
    {
        //Task<IEnumerable<VehiclePrices>> GetPricesByVehicleIdAsync(int vehicleId);
        //Task<IEnumerable<VehiclePrices>> GetPriceHistoryAsync(int vehicleId);
        //Task<IEnumerable<VehiclePrices>> GetPricesByAgencyAsync(int agencyId);
    }
}
