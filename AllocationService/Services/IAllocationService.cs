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
        Task<IEnumerable<AllocationResponse>> GetByAgencyContractIdAsync(int agencyContractId);
        Task<AllocationResponse?> GetByAgencyContractAndVehicleAsync(int agencyContractId, int vehicleInstanceId);
        Task<IEnumerable<AllocationResponse>> GetByVehicleInstanceIdAsync(int vehicleInstanceId);
        Task<AllocationResponse> UpdateAsync(int id, AllocationRequestModel request);
        Task<bool> DeleteAsync(int id);
        //Task<AllocationResponse?> GetByInventoryIdAsync(int evInventoryId);
    }
}
