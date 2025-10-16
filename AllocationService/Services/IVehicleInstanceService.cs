using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IVehicleInstanceService
    {
        Task<IEnumerable<VehicleInstanceResponse>> GetAllAsync();
        Task<VehicleInstanceResponse?> GetByIdAsync(int id);
        Task<IEnumerable<VehicleInstanceResponse>> GetByVehicleIdAsync(int vehicleId);
        Task<VehicleInstanceResponse> CreateAsync(CreateVehicleInstanceRequest instance);
        Task<VehicleInstanceResponse> UpdateAsync(int id, UpdateVehicleInstanceRequest instance);
        Task<bool> DeleteAsync(int id);
    }
}
