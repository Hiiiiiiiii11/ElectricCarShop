using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public class VehiclePriceService : IVehiclePriceService
    {
        private readonly IVehiclePriceRepository _vehiclePriceRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public VehiclePriceService(IVehiclePriceRepository vehiclePriceRepository)
        {
            _vehiclePriceRepository = vehiclePriceRepository;
        }

        public async Task<VehiclePriceResponse> CreateAsync(VehiclePriceRequest request)
        {
            var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleId);
            if (vehicle == null)
                throw new KeyNotFoundException("Vehicle not found.");
            var entity = new VehiclePrices
            {
                VehicleId = request.VehicleId,
                AgencyId = request.AgencyId,
                PriceType = request.PriceType,
                PriceAmount = request.PriceAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            await _vehiclePriceRepository.AddAsync(entity);
            await _vehiclePriceRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }
        public async Task<VehiclePriceResponse> UpdateAsync(int id, VehiclePriceUpdateRequest request)
        {
            var entity = await _vehiclePriceRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Vehicle price not found.");

            // Chỉ cập nhật nếu có giá trị mới
            entity.VehicleId = request.VehicleId ?? entity.VehicleId;
            entity.AgencyId = request.AgencyId ?? entity.AgencyId;
            entity.PriceType = request.PriceType ?? entity.PriceType;
            entity.PriceAmount = request.PriceAmount ?? entity.PriceAmount;
            entity.StartDate = request.StartDate ?? entity.StartDate;
            entity.EndDate = request.EndDate ?? entity.EndDate;

            _vehiclePriceRepository.Update(entity);
            await _vehiclePriceRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _vehiclePriceRepository.GetByIdAsync(id);
            if (entity == null) throw new KeyNotFoundException("Vehicle price not found.");
            _vehiclePriceRepository.Remove(entity);
            await _vehiclePriceRepository.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<VehiclePriceResponse>> GetAllAsync()
        {
            var entities = await _vehiclePriceRepository.GetAllAsync();
            return entities.Select(MapToResponse);
        }

        public async Task<VehiclePriceResponse> GetByIdAsync(int id)
        {
            var entity = await _vehiclePriceRepository.GetByIdAsync(id);
            if(entity == null) throw new KeyNotFoundException("Vehicle price not found.");
            return MapToResponse(entity);

        }

        public async Task<IEnumerable<VehiclePriceResponse>> GetPriceHistoryAsync(int vehicleId)
        {
            var entities = await _vehiclePriceRepository.GetPriceHistoryAsync(vehicleId);
            return entities.Select(MapToResponse);
        }

        public async Task<IEnumerable<VehiclePriceResponse>> GetPricesByAgencyAsync(int agencyId)
        {
            var entities = await _vehiclePriceRepository.GetPricesByAgencyAsync(agencyId);
            return entities.Select(MapToResponse);
        }

        public async Task<IEnumerable<VehiclePriceResponse>> GetPricesByVehicleIdAsync(int vehicleId)
        {
            var entities = await _vehiclePriceRepository.GetPricesByVehicleIdAsync(vehicleId);
            return entities.Select(MapToResponse);
        }

        public VehiclePriceResponse MapToResponse(VehiclePrices prices)
        {
            return new VehiclePriceResponse
            {
                Id = prices.Id,
                VehicleId = prices.VehicleId,
                AgencyId = prices.AgencyId,
                PriceType = prices.PriceType,
                PriceAmount = prices.PriceAmount,
                StartDate = prices.StartDate,
                EndDate = prices.EndDate,
            };
        }
    }
}
