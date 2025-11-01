using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using Azure.Core;
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
        private readonly IEVInventoryService _evInventoryService;

        public AllocationService(
            IAllocationRepository allocationRepository,
            IAgencyGrpcServiceClient agencyGrpcClient,
            IEVInventoryService evInventoryService)
        {
            _allocationRepository = allocationRepository;
            _agencyGrpcClient = agencyGrpcClient;
            _evInventoryService = evInventoryService;
        }

        public async Task<AllocationResponse> CreateAsync(AllocationRequestModel request)
        {
            // Lấy thông tin hợp đồng và đại lý qua gRPC
            var contract = await _agencyGrpcClient.GetContractByIdAsync(request.AgencyContractId);
            if (contract == null)
                throw new KeyNotFoundException($"Không tìm thấy hợp đồng với ID {request.AgencyContractId}");
            var vehicleInInventory = await _evInventoryService.GetByVehicleInstanceIdAsync(request.VehicleInstanceId);
            if (vehicleInInventory == null)
                throw new KeyNotFoundException($"Không tìm thấy xe trong kho với ID {request.VehicleInstanceId}");
            var allocation = new Allocations
            {
                AgencyContractId = request.AgencyContractId,
                VehicleInstanceId = request.VehicleInstanceId,
                AllocationDate = DateTime.UtcNow
            };

            await _allocationRepository.AddAsync(allocation);
            await _allocationRepository.SaveChangesAsync();
            await _evInventoryService.DeleteByVehicleInstanceIdAsync(request.VehicleInstanceId);
            var response = MapToResponse(allocation);
            response.ContractReply = contract;

            return response;
        }
        public async Task<AllocationResponse> UpdateAsync(int id, AllocationRequestModel request)
        {
            var allocation = await _allocationRepository.GetByIdAsync(id);
            if (allocation == null)
                throw new KeyNotFoundException($"Không tìm thấy phân phối với ID {id}");

            // Cập nhật dữ liệu
            allocation.AgencyContractId = request.AgencyContractId;
            allocation.VehicleInstanceId = request.VehicleInstanceId;
            allocation.AllocationDate = DateTime.UtcNow;

            _allocationRepository.Update(allocation);
            await _allocationRepository.SaveChangesAsync();

            // Có thể gọi lại gRPC để làm giàu dữ liệu
            var response = MapToResponse(allocation);
            return response;
        }

        public async Task<IEnumerable<AllocationResponse>> GetByAgencyContractIdAsync(int agencyContractId)
        {
            var allocations = await _allocationRepository.GetByAgencyIdAsync(agencyContractId);
            if (allocations == null)
                throw new KeyNotFoundException($"Không tìm thấy hợp đông đại lý với ID {agencyContractId}");

            var agencycontract = await _agencyGrpcClient.GetContractByIdAsync(agencyContractId);

            return allocations.Select(a =>
            {
                var res = MapToResponse(a);
                res.ContractReply = agencycontract;
                return res;
            });
        }
        public async Task<bool> DeleteAsync(int id)
        {
            var allocation = await _allocationRepository.GetByIdAsync(id);
            if (allocation == null)
                throw new KeyNotFoundException($"Không tìm thấy phân phối với ID {id}");

            _allocationRepository.Remove(allocation);
            await _allocationRepository.SaveChangesAsync();
            return true;
        }

        public async Task<AllocationResponse?> GetByAgencyContractAndVehicleAsync(int agencyContractId, int vehicleInstanceId)
        {
            var entity = await _allocationRepository.GetByAgencyAndVehicleInstanceAsync(agencyContractId, vehicleInstanceId);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy đại lý với ID {agencyContractId} hoặc xe với ID {vehicleInstanceId}");

            var agencycontract = await _agencyGrpcClient.GetContractByIdAsync(agencyContractId);
            var response = MapToResponse(entity);
            response.ContractReply = agencycontract;

            return response;
        }

        //public async Task<AllocationResponse?> GetByInventoryIdAsync(int evInventoryId)
        //{
        //    var entity = await _allocationRepository.GetByInventoryIdAsync(evInventoryId);
        //    if (entity == null)
        //    {
        //        throw new KeyNotFoundException($"Không tìm thấy kho với ID {evInventoryId}");
        //    }

        //    var agency = await _agencyGrpcClient.GetAgencyByIdAsync(entity.AgencyId);
        //    var response = MapToResponse(entity);
        //    response.AgencyName = agency?.AgencyName;
        //    response.AgencyEmail = agency?.Email;

        //    return response;
        //}

        public async Task<IEnumerable<AllocationResponse>> GetByVehicleInstanceIdAsync(int vehicleInstanceId)
        {
            var entities = await _allocationRepository.GetByVehicleInstanceIdAsync(vehicleInstanceId);
            if(entities == null)
            {
                throw new KeyNotFoundException($"Không tìm thấy xe với ID {vehicleInstanceId}");
            }

            var result = new List<AllocationResponse>();
            foreach (var entity in entities)
            {
                var agencycontract = await _agencyGrpcClient.GetContractByIdAsync(entity.AgencyContractId);
                var res = MapToResponse(entity);
                res.ContractReply = agencycontract;
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
                AgencyContractId = a.AgencyContractId,
                VehicleInstanceId = a.VehicleInstanceId,
                AllocationDate = a.AllocationDate,
                VehicleInstance = a.VehicleInstance == null ? null : new VehicleInstanceResponse
                {
                    Id = a.VehicleInstance.Id,
                    VehicleId = a.VehicleInstance.VehicleId,
                    Vin = a.VehicleInstance.Vin,
                    EngineNumber = a.VehicleInstance.EngineNumber
                },
            };
        }
    }
}
