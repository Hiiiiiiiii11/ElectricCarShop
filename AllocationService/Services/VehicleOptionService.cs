using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public class VehicleOptionService : IVehicleOptionService
    {
        private readonly IVehicleOptionRepository _vehicleOptionRepository;
        public VehicleOptionService(IVehicleOptionRepository vehicleOptionRepository)
        {
            _vehicleOptionRepository = vehicleOptionRepository;
        }
        public async Task<VehicleOptionResponse> CreateAsync(VehicleOptionRequest request)
        {
            var option = new VehicleOptions
            {
                ModelName = request.ModelName,
                Description = request.Description,
                CreateAt = DateTime.UtcNow,
                UpdateAt = DateTime.UtcNow
            };
            await _vehicleOptionRepository.AddAsync(option);
            await _vehicleOptionRepository.SaveChangesAsync();
            return MapToResponse(option);

        }

        public async Task DeleteAsync(int id)
        {
            var option = await _vehicleOptionRepository.GetByIdAsync(id);
            if (option == null)
                throw new KeyNotFoundException($"Vehicle option {id} not found");
            _vehicleOptionRepository.Remove(option);
            await _vehicleOptionRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<VehicleOptionResponse>> GetAllAsync()
        {
            var options =  await _vehicleOptionRepository.GetAllAsync();
            return options.Select(MapToResponse);
        }

        public async Task<VehicleOptionResponse?> GetByIdAsync(int id)
        {
            var option = await _vehicleOptionRepository.GetByIdAsync(id);
            return option == null ? null : MapToResponse(option);
        }

        public  async Task<VehicleOptionResponse> GetByModelNameAsync(string modelName)
        {
            var entity = await _vehicleOptionRepository.GetByModelNameAsync(modelName);
            return entity == null ? null : MapToResponse(entity);
        }

        public async Task<VehicleOptionResponse?> UpdateAsync(int vehicleOptionId,UpdateVehicleOptionRequest request)
        {
            var entity = await _vehicleOptionRepository.GetByIdAsync(vehicleOptionId);
            if (entity == null) return null;

            // Nếu request.ModelName có giá trị thì mới update, không thì giữ nguyên
            if (!string.IsNullOrWhiteSpace(request.ModelName))
                entity.ModelName = request.ModelName;

            if (!string.IsNullOrWhiteSpace(request.Description))
                entity.Description = request.Description;

            entity.UpdateAt = DateTime.UtcNow;

            _vehicleOptionRepository.Update(entity);
            await _vehicleOptionRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public VehicleOptionResponse MapToResponse(VehicleOptions option)
        {
            return new VehicleOptionResponse
            {
                Id = option.Id,
                ModelName = option.ModelName,
                Description = option.Description,
                CreateAt = option.CreateAt,
                UpdateAt = option.UpdateAt,
            };
        }
    }
}
