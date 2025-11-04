using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public class VehiclePromotionService : IVehiclePromotionService
    {
        private readonly IVehiclePromotionRepository _vehiclePromotionRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public VehiclePromotionService(
            IVehiclePromotionRepository vehiclePromotionRepository,
            IVehicleRepository vehicleRepository)
        {
            _vehiclePromotionRepository = vehiclePromotionRepository;
            _vehicleRepository = vehicleRepository;
        }

        // ------------------- CRUD -------------------

        public async Task<VehiclePromotionResponse> CreateAsync(VehiclePromotionRequest request)
        {
            var entity = new VehiclePromotions
            {
                VehicleId = request.VehicleId,
                AgencyId = request.AgencyId,
                PromoName = request.PromoName,
                DiscountAmount = request.DiscountAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate
            };

            await _vehiclePromotionRepository.AddAsync(entity);
            await _vehiclePromotionRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<VehiclePromotionResponse> UpdateAsync(int id, VehiclePromotionUpdateRequest request)
        {
            var entity = await _vehiclePromotionRepository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Vehicle promotion not found.");

            // Giữ lại giá trị cũ nếu null
            entity.VehicleId = request.VehicleId ?? entity.VehicleId;
            entity.PromoName = request.PromoName ?? entity.PromoName;
            entity.AgencyId = request.AgencyId ?? entity.AgencyId;
            entity.DiscountAmount = request.DiscountAmount ?? entity.DiscountAmount;
            entity.StartDate = request.StartDate ?? entity.StartDate;
            entity.EndDate = request.EndDate ?? entity.EndDate;

            _vehiclePromotionRepository.Update(entity);
            await _vehiclePromotionRepository.SaveChangesAsync();

            return MapToResponse(entity);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _vehiclePromotionRepository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Vehicle promotion not found.");

            _vehiclePromotionRepository.Remove(entity);
            await _vehiclePromotionRepository.SaveChangesAsync();
            return true;
        }


        public async Task<IEnumerable<VehiclePromotionResponse>> GetAllAsync()
        {
            var list = await _vehiclePromotionRepository.GetAllAsync();
            return list.Select(MapToResponse);
        }

        public async Task<VehiclePromotionResponse> GetByIdAsync(int id)
        {
            var entity = await _vehiclePromotionRepository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException("Vehicle promotion not found.");

            return MapToResponse(entity);
        }

        public async Task<IEnumerable<VehiclePromotionResponse>> GetActivePromotionsAsync()
        {
            var list = await _vehiclePromotionRepository.GetActivePromotionsAsync();
            return list.Select(MapToResponse);
        }

        public async Task<IEnumerable<VehiclePromotionResponse>> GetExpiredPromotionsAsync()
        {
            var list = await _vehiclePromotionRepository.GetExpiredPromotionsAsync();
            return list.Select(MapToResponse);
        }

        public async Task<IEnumerable<VehiclePromotionResponse>> GetPromotionsByVehicleIdAsync(int vehicleId)
        {
            var list = await _vehiclePromotionRepository.GetPromotionsByVehicleIdAsync(vehicleId);
            return list.Select(MapToResponse);
        }
        public async Task<IEnumerable<VehiclePromotionResponse>> GetPromotionByAgencyIdAsync(int agencyId)
        {
            var list = await _vehiclePromotionRepository.GetPromotionByAgencyIdAsync(agencyId);
            return list.Select(MapToResponse);
        }

        private VehiclePromotionResponse MapToResponse(VehiclePromotions entity)
        {
            return new VehiclePromotionResponse
            {
                Id = entity.Id,
                VehicleId = entity.VehicleId,
                AgencyId = entity.AgencyId ?? 0,
                PromoName = entity.PromoName,
                DiscountAmount = entity.DiscountAmount,
                StartDate = entity.StartDate,
                EndDate = entity.EndDate
            };
        }
    }
}
