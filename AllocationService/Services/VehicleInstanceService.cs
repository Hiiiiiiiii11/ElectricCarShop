using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public class VehicleInstanceService : IVehicleInstanceService
    {
        private readonly IVehicleInstanceRepository _vehicleInstanceRepository;

        public VehicleInstanceService(IVehicleInstanceRepository vehicleInstanceRepository)
        {
            _vehicleInstanceRepository = vehicleInstanceRepository;
        }

        // ================== CREATE ==================
        public async Task<VehicleInstanceResponse> CreateAsync(CreateVehicleInstanceRequest instance)
        {
            if (await _vehicleInstanceRepository.IsVinExistAsync(instance.Vin))
                throw new Exception("Số khung (VIN) đã tồn tại trong hệ thống.");

            if (await _vehicleInstanceRepository.IsEngineNumberExistAsync(instance.EngineNumber))
                throw new Exception("Số máy đã tồn tại trong hệ thống.");

            var entity = new VehicleInstance
            {
                VehicleId = instance.VehicleId,
                Vin = instance.Vin,
                EngineNumber = instance.EngineNumber,
            };

            await _vehicleInstanceRepository.AddAsync(entity);
            await _vehicleInstanceRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        // ================== UPDATE ==================
        public async Task<VehicleInstanceResponse> UpdateAsync(int id, UpdateVehicleInstanceRequest instance)
        {
            var entity = await _vehicleInstanceRepository.GetByIdAsync(id);
            if (entity == null)
                throw new Exception("Không tìm thấy xe cần cập nhật.");

            // ✅ Nếu có truyền VIN mới và khác với VIN cũ → kiểm tra trùng
            if (!string.IsNullOrWhiteSpace(instance.Vin) && entity.Vin != instance.Vin)
            {
                if (await _vehicleInstanceRepository.IsVinExistAsync(instance.Vin))
                    throw new Exception("Số khung (VIN) đã tồn tại trong hệ thống.");
                entity.Vin = instance.Vin; // cập nhật VIN mới
            }

            // ✅ Nếu có truyền EngineNumber mới và khác với EngineNumber cũ → kiểm tra trùng
            if (!string.IsNullOrWhiteSpace(instance.EngineNumber) && entity.EngineNumber != instance.EngineNumber)
            {
                if (await _vehicleInstanceRepository.IsEngineNumberExistAsync(instance.EngineNumber))
                    throw new Exception("Số máy đã tồn tại trong hệ thống.");
                entity.EngineNumber = instance.EngineNumber; // cập nhật số máy mới
            }

            // ✅ VehicleId chỉ cập nhật nếu được truyền
            if (instance.VehicleId.HasValue)
                entity.VehicleId = instance.VehicleId.Value;
             _vehicleInstanceRepository.Update(entity);
            await _vehicleInstanceRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }



        // ================== GET ALL ==================
        public async Task<IEnumerable<VehicleInstanceResponse>> GetAllAsync()
        {
            var list = await _vehicleInstanceRepository.GetAllAsync();
            return list.Select(MapToResponse);
        }

        // ================== GET BY ID ==================
        public async Task<VehicleInstanceResponse?> GetByIdAsync(int id)
        {
            var entity = await _vehicleInstanceRepository.GetByIdAsync(id);
            return entity == null ? null : MapToResponse(entity);
        }

        // ================== GET BY VEHICLE ID ==================
        public async Task<IEnumerable<VehicleInstanceResponse>> GetByVehicleIdAsync(int vehicleId)
        {
            var list = await _vehicleInstanceRepository.GetByVehicleIdAsync(vehicleId);
            return list.Select(MapToResponse);
        }

        // ================== DELETE ==================
        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _vehicleInstanceRepository.GetByIdAsync(id);
            if (entity == null)
                return false;
             _vehicleInstanceRepository.Remove(entity);
            await _vehicleInstanceRepository.SaveChangesAsync();
            return true;
        }

        // ================== MAP TO RESPONSE ==================
        private VehicleInstanceResponse MapToResponse(VehicleInstance instance)
        {
            return new VehicleInstanceResponse
            {
                Id = instance.Id,
                VehicleId = instance.VehicleId,
                Vin = instance.Vin,
                EngineNumber = instance.EngineNumber,
            };
        }
    }
}
