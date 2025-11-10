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
        Task<IEnumerable<Allocations>> GetByAgencyIdAsync(int agencycontractId);

        // Lấy tất cả allocation theo Vehicle
        Task<IEnumerable<Allocations>> GetByVehicleInstanceIdAsync(int vehicleInstanceId);


        // Tìm allocation theo Agency + Vehicle
        Task<Allocations?> GetByAgencyAndVehicleInstanceAsync(int agencycontractId, int vehicleInstanceId);
        Task<IEnumerable<Allocations>> GetByAgencyOrderIdAsync(int agencyOrderId);
        Task<IEnumerable<Allocations>> GetAllWithDetailsAsync();
    }
}
