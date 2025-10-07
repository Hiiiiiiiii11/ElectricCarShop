using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IAllocationService
    {
        Task<AllocationResponse> CreateAsync(AllocationRequestModel request);
        Task<IEnumerable<AllocationResponse>> GetByAgencyIdAsync(int agencyId);
        Task<AllocationResponse?> GetByAgencyAndVehicleAsync(int agencyId, int vehicleId);
        Task<IEnumerable<AllocationResponse>> GetByVehicleIdAsync(int vehicleId);
        Task<AllocationResponse?> GetByInventoryIdAsync(int evInventoryId);
    }
}
