using AllocationRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IVehicleInstanceRepository :IGenericRepository<VehicleInstance>
    {
        Task<IEnumerable<VehicleInstance>> GetByVehicleIdAsync(int vehicleId);
        Task<VehicleInstance?> GetByVinAsync(string vin);
        Task<VehicleInstance?> GetByEngineNumberAsync(string engineNumber);
        Task<bool> IsVinExistAsync(string vin);
        Task<bool> IsEngineNumberExistAsync(string engineNumber);
        Task<int> CountByVehicleIdAsync(int vehicleId);
        Task<VehicleInstance> GetByIdWithDetailsAsync(int id);
    }
}
