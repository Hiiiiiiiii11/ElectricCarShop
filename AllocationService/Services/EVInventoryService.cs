using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using CloudinaryDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public class EVInventoryService : IEVInventoryService
    {
        private readonly IEVInventoryRepository _evInventoryRepository;
        private readonly IVehicleInstanceRepository _vehicleInstanceRepository;
        public EVInventoryService(IEVInventoryRepository evInventoryRepository, IVehicleInstanceRepository vehicleInstanceRepository)
        {
            _evInventoryRepository = evInventoryRepository;
            _vehicleInstanceRepository = vehicleInstanceRepository;
        }

        public async Task<EVInventoryResponse> CreateInventoryAsync(EVInventoryRequest request)
        {
            // Đã bỏ phần kiểm tra tồn tại (if existing != null) theo yêu cầu.
            var vehicleInstance = await _vehicleInstanceRepository.GetByIdAsync(request.VehicleInstanceId);
            if (vehicleInstance == null)
                throw new KeyNotFoundException($"Không tìm thấy VehicleInstance với ID = {request.VehicleInstanceId}");

            var inv = new EVInventory
            {
                VehicleInstanceId = request.VehicleInstanceId
                // Trường Quantity cũng đã được bỏ đi vì mỗi VehicleInstance là duy nhất.
            };

            await _evInventoryRepository.AddAsync(inv);
            await _evInventoryRepository.SaveChangesAsync();

            return MapToResponse(inv);
        }
        //public async Task IncreaseInventoryAsync(int vehicleId, int quantity)
        //{
        //    if (quantity <= 0)
        //        throw new ArgumentException("Số lượng phải lớn hơn 0.");

        //    await _evInventoryRepository.IncreaseQuantityAsync(vehicleId, quantity);
        //}
        //public async  Task DecreaseInventoryAsync(int vehicleId, int quantity)
        //{
        //    if (quantity <= 0)
        //        throw new ArgumentException("Số lượng phải lớn hơn 0.");

        //    bool hasEnough = await _evInventoryRepository.HasEnoughStockAsync(vehicleId, quantity);
        //    if (!hasEnough)
        //        throw new InvalidOperationException("Không đủ xe trong kho để phân bổ.");

        //    await _evInventoryRepository.DecreaseQuantityAsync(vehicleId, quantity);
        //}

        public async Task DeleteInventoryAsync(int id)
        {
            var inv = await _evInventoryRepository.GetByIdAsync(id);
            if (inv == null)
                throw new KeyNotFoundException($"Inventory not found Id = {id}");

            _evInventoryRepository.Remove(inv);
            await _evInventoryRepository.SaveChangesAsync();
        }

        public async Task DeleteByVehicleInstanceIdAsync(int vehicleInstanceId)
        {
            var inv = await _evInventoryRepository.GetByVehicleInstanceIdAsync(vehicleInstanceId);
            if (inv == null)
                throw new KeyNotFoundException($"Không tìm thấy xe trong kho với VehicleInstanceId = {vehicleInstanceId}");

            _evInventoryRepository.Remove(inv);
            await _evInventoryRepository.SaveChangesAsync();
        }
        public async Task<IEnumerable<EVInventoryResponse>> GetAllInventoriesAsync()
        {
            var list = await _evInventoryRepository.GetAllWithVehiclesAsync();
            return list.Select(MapToResponse);
        }

        public async Task<EVInventoryResponse?> GetByIdAsync(int id)
        {
            var inv = await _evInventoryRepository.GetByIdAsync(id);
            if (inv == null)
            {
                throw new KeyNotFoundException($"Inventory with Id = {id} not found");
            }
            return MapToResponse(inv);
        }
        public async Task<EVInventoryResponse?> GetByVehicleInstanceIdAsync(int vehicleInstanceId)
        {
            var inv = await _evInventoryRepository.GetByVehicleInstanceIdAsync(vehicleInstanceId);
            if (inv == null)
            {
                throw new KeyNotFoundException($"Inventory for vehicle instance {vehicleInstanceId} not found");
            }
            return inv == null ? null : MapToResponse(inv);
        }

        public async Task<EVInventoryResponse?> GetByVehicleIdAsync(int vehicleId)
        {
            var inv = await _evInventoryRepository.GetByVehicleInstanceIdAsync(vehicleId);
            if (inv == null)
            {
                throw new KeyNotFoundException($"Inventory for vehicle {vehicleId} not found");
            }
            return inv == null ? null : MapToResponse(inv);
        }

        //public async Task<int> GetTotalInventoryAsync()
        //{
        //   return await _evInventoryRepository.GetTotalInventoryCountAsync();
        //}

        //public async Task<bool> HasEnoughStockAsync(int vehicleId, int requiredQuantity)
        //{
        //    return await _evInventoryRepository.HasEnoughStockAsync(vehicleId, requiredQuantity);
        //}


        private EVInventoryResponse MapToResponse(EVInventory inv)
        {
            return new EVInventoryResponse
            {
                Id = inv.Id,
                VehicleInstanceId = inv.VehicleInstanceId,

                 VehicleInstance = inv.VehicleInstance == null ? null : new VehicleInstanceResponse
                 {
                     Id = inv.VehicleInstance.Id,
                     VehicleId = inv.VehicleInstance.VehicleId,
                     Vin = inv.VehicleInstance.Vin,
                     EngineNumber = inv.VehicleInstance.EngineNumber
                 },
            };
        }
    }
}
