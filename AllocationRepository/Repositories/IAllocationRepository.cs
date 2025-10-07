using AllocationRepository.Model;
using Share.ShareRepo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationRepository.Repositories
{
    public interface IAllocationRepository :IGenericRepository<Allocations>
    {
        // Lấy tất cả allocation theo Agency
        Task<IEnumerable<Allocations>> GetByAgencyIdAsync(int agencyId);

        // Lấy tất cả allocation theo Vehicle
        Task<IEnumerable<Allocations>> GetByVehicleIdAsync(int vehicleId);

        // Lấy allocation cụ thể theo Inventory
        Task<Allocations?> GetByInventoryIdAsync(int evInventoryId);

        // Tìm allocation theo Agency + Vehicle
        Task<Allocations?> GetByAgencyAndVehicleAsync(int agencyId, int vehicleId);
    }
}
