using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly IAgencyGrpcServiceClient _agencyGrpcClient;

        public AllocationService(
            IAllocationRepository allocationRepository,
            IAgencyGrpcServiceClient agencyGrpcClient)
        {
            _allocationRepository = allocationRepository;
            _agencyGrpcClient = agencyGrpcClient;
        }

        public async Task<AllocationResponse> CreateAsync(AllocationRequestModel request)
        {
            var agency = await _agencyGrpcClient.GetAgencyByIdAsync(request.AgencyId);
            if (agency == null)
                throw new Exception($"Không tìm thấy đại lý với ID {request.AgencyId}");

            var allocation = new Allocations
            {
                AgencyId = request.AgencyId,
                EvInventoryId = request.EvInventoryId,
                VehicleId = request.VehicleId,
                AllocationQuantity = request.AllocationQuantity,
                AllocationDate = DateTime.UtcNow
            };

            await _allocationRepository.AddAsync(allocation);
            await _allocationRepository.SaveChangesAsync();

            // Trả về thông qua mapping
            var response = MapToResponse(allocation);
            response.AgencyName = agency.AgencyName;
            response.AgencyEmail = agency.Email;

            return response;
        }

        public async Task<IEnumerable<AllocationResponse>> GetByAgencyIdAsync(int agencyId)
        {
            var allocations = await _allocationRepository.GetByAgencyIdAsync(agencyId);
            var agency = await _agencyGrpcClient.GetAgencyByIdAsync(agencyId);

            return allocations.Select(a =>
            {
                var res = MapToResponse(a);
                res.AgencyName = agency?.AgencyName;
                res.AgencyEmail = agency?.Email;
                return res;
            });
        }

        public async Task<AllocationResponse?> GetByAgencyAndVehicleAsync(int agencyId, int vehicleId)
        {
            var entity = await _allocationRepository.GetByAgencyAndVehicleAsync(agencyId, vehicleId);
            if (entity == null) return null;

            var agency = await _agencyGrpcClient.GetAgencyByIdAsync(agencyId);
            var response = MapToResponse(entity);
            response.AgencyName = agency?.AgencyName;
            response.AgencyEmail = agency?.Email;

            return response;
        }

        public async Task<AllocationResponse?> GetByInventoryIdAsync(int evInventoryId)
        {
            var entity = await _allocationRepository.GetByInventoryIdAsync(evInventoryId);
            if (entity == null) return null;

            var agency = await _agencyGrpcClient.GetAgencyByIdAsync(entity.AgencyId);
            var response = MapToResponse(entity);
            response.AgencyName = agency?.AgencyName;
            response.AgencyEmail = agency?.Email;

            return response;
        }

        public async Task<IEnumerable<AllocationResponse>> GetByVehicleIdAsync(int vehicleId)
        {
            var entities = await _allocationRepository.GetByVehicleIdAsync(vehicleId);

            var result = new List<AllocationResponse>();
            foreach (var entity in entities)
            {
                var agency = await _agencyGrpcClient.GetAgencyByIdAsync(entity.AgencyId);
                var res = MapToResponse(entity);
                res.AgencyName = agency?.AgencyName;
                res.AgencyEmail = agency?.Email;
                result.Add(res);
            }

            return result;
        }

        // ====== MAPPING CHUNG ======
        private AllocationResponse MapToResponse(Allocations a)
        {
            return new AllocationResponse
            {
                Id = a.Id,
                AgencyId = a.AgencyId,
                EvInventoryId = a.EvInventoryId,
                VehicleId = a.VehicleId,
                AllocationQuantity = a.AllocationQuantity,
                AllocationDate = a.AllocationDate,

                // Nếu muốn include thêm thông tin liên quan thì mở comment phần dưới:
                //Vehicle = a.Vehicle == null ? null : new VehicleResponse
                //{
                //    Id = a.Vehicle.Id,
                //    VariantName = a.Vehicle.VariantName,
                //    Color = a.Vehicle.Color,
                //    BatteryCapacity = a.Vehicle.BatteryCapacity,
                //    RangeKM = a.Vehicle.RangeKM,
                //    Features = a.Vehicle.Features,
                //    Status = a.Vehicle.Status
                //},
                //EVInventory = a.EVInventory == null ? null : new EVInventoryResponse
                //{
                //    Id = a.EVInventory.Id,
                //    Quantity = a.EVInventory.Quantity,
                //    ModelName = a.EVInventory.ModelName
                //}
            };
        }
    }
}
