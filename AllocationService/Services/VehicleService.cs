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
    public class VehicleService : IVehicleService
    {
        private readonly IVehicleRepository _vehicleRepository;
        public VehicleService(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }
        public async Task<VehicleResponse> AddVehicleAsync(CreateVehicleRequest request)
        {
            var vehicle = new Vehicles
            {
                VariantName = request.VariantName,
                VehicleOptionId = request.VehicleOptionId,
                Color = request.Color,
                BatteryCapacity = request.BatteryCapacity,
                Features = request.Features,
                RangeKM = request.RangeKM,
                Status = request.Status,
            };
            await _vehicleRepository.AddAsync(vehicle);
            await _vehicleRepository.SaveChangesAsync();
            return MapToResponse(vehicle);
        }
        public async Task UpdateVehicleAsync(int vehicleId, UpdateVehicleRequest request)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(vehicleId);
            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle {vehicleId} not found");
            if (!string.IsNullOrEmpty(request.VariantName))
                vehicle.VariantName = request.VariantName;

            if (!string.IsNullOrEmpty(request.Color))
                vehicle.Color = request.Color;

            if (!string.IsNullOrEmpty(request.BatteryCapacity))
                vehicle.BatteryCapacity = request.BatteryCapacity;

            if (request.RangeKM.HasValue)
                vehicle.RangeKM = request.RangeKM.Value;

            if (!string.IsNullOrEmpty(request.Features))
                vehicle.Features = request.Features;

            if (!string.IsNullOrEmpty(request.Status))
                vehicle.Status = request.Status;

            if (request.VehicleOptionId.HasValue)
                vehicle.VehicleOptionId = request.VehicleOptionId.Value;

            _vehicleRepository.Update(vehicle);
            await _vehicleRepository.SaveChangesAsync();
        }


        public async Task DeleteVehicleAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle {id} not found");
            _vehicleRepository.Remove(vehicle);
            await _vehicleRepository.SaveChangesAsync();
        }

        public async Task<IEnumerable<VehicleResponse>> GetAllVehiclesAsync()
        {
            var vehicles = await _vehicleRepository.GetAllAsync();
            return vehicles.Select(MapToResponse);
        }

        public Task<decimal?> GetCurrentPriceAsync(int vehicleId, DateTime date)
        {
            var price = _vehicleRepository.GetCurrentPriceAsync(vehicleId, date);
            return price;
        }

        public async Task<VehicleResponse?> GetVehicleByIdAsync(int id)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(id);
            return vehicle == null ? null : MapToResponse(vehicle);
        }

        public async Task<IEnumerable<VehicleResponse>> GetVehiclesByStatusAsync(string status)
        {
            var vehicles = await _vehicleRepository.FindAsync(v => v.Status == status);
            return vehicles.Select(MapToResponse);
        }

        public async Task<IEnumerable<VehicleResponse>> GetVehiclesWithAvailableStockAsync()
        {
            var vehicles = await _vehicleRepository.GetVehiclesWithAvailableStockAsync();
            return vehicles.Select(MapToResponse);
        }

        public async Task<IEnumerable<VehicleResponse>> SearchVehiclesAsync(string? variantName, string? color, string? batteryCapacity)
        {
            var vehicles = await _vehicleRepository.SearchVehiclesAsync(variantName, color, batteryCapacity);
            return vehicles.Select(MapToResponse);
        }

        private VehicleResponse MapToResponse(Vehicles v)
        {
            return new VehicleResponse
            {
                Id = v.Id,
                VehicleOptionId = v.VehicleOptionId,
                VariantName = v.VariantName,
                Color = v.Color,
                BatteryCapacity = v.BatteryCapacity,
                RangeKM = v.RangeKM,
                Features = v.Features,
                Status = v.Status,
                Option = v.VehicleOption == null ? null : new VehicleOptionResponse
                {
                    Id = v.VehicleOption.Id,
                    Description = v.VehicleOption.Description
                },
                Allocations = v.Allocations.Select(a => new AllocationResponse
                {
                    Id = a.Id,
                    DealerId = a.DealerId,
                    AllocationQuantity = a.AllocationQuantity,
                    AllocationDate = a.AllocationDate
                }).ToList()
            };
        }

    }
}
