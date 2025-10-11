using AllocationRepository.Model.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public interface IVehicleOptionService
    {
        Task<IEnumerable<VehicleOptionResponse>> GetAllAsync();
        Task<VehicleOptionResponse?> GetByIdAsync(int id);
        Task<VehicleOptionResponse> CreateAsync(VehicleOptionRequest request);
        Task<VehicleOptionResponse?> UpdateAsync(int vehicleOptionId, UpdateVehicleOptionRequest request);
        Task DeleteAsync(int id);
        Task<VehicleOptionResponse> GetByModelNameAsync(string modelName);

    }
}
