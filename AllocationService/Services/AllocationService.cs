using AllocationRepository.Model;
using AllocationRepository.Model.DTO;
using AllocationRepository.Repositories;
using Azure.Core;
using GrpcService;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;

namespace AllocationService.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly IAllocationRepository _allocationRepository;
        private readonly IAgencyGrpcServiceClient _agencyGrpcClient;
        private readonly IEVInventoryService _evInventoryService;
        private readonly IVehicleInstanceRepository _vehicleInstanceRepository;

        public AllocationService(
            IAllocationRepository allocationRepository,
            IAgencyGrpcServiceClient agencyGrpcClient,
            IEVInventoryService evInventoryService,
            IVehicleInstanceRepository vehicleInstanceRepository)
        {
            _allocationRepository = allocationRepository;
            _agencyGrpcClient = agencyGrpcClient;
            _evInventoryService = evInventoryService;
            _vehicleInstanceRepository = vehicleInstanceRepository;
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
                AgencyOrderId = request.AgencyOrderId,
                AllocationDate = DateTime.UtcNow,

            };

            await _allocationRepository.AddAsync(allocation);
            await _allocationRepository.SaveChangesAsync();
            await _evInventoryService.DeleteByVehicleInstanceIdAsync(request.VehicleInstanceId);
            var vehicleInstance = await _vehicleInstanceRepository.GetByIdAsync(request.VehicleInstanceId);
            if (vehicleInstance != null)
            {
                // Ví dụ encode vào status: IN_AGENCY_{AgencyId}
                vehicleInstance.Status = $"IN_AGENCY_{contract.AgencyId}";

                _vehicleInstanceRepository.Update(vehicleInstance);
                await _vehicleInstanceRepository.SaveChangesAsync();
            }
            var response = MapToResponse(allocation);
            response.ContractReply = contract;

            return response;
        }
        public async Task<AllocationResponse> UpdateAsync(int id, AllocationUpdateModel request)
        {
            var allocation = await _allocationRepository.GetByIdAsync(id);
            if (allocation == null)
                throw new KeyNotFoundException($"Không tìm thấy phân phối với ID {id}");

            // --- Cập nhật giữ nguyên giá trị cũ nếu không truyền ---

            allocation.AgencyContractId = request.AgencyContractId.HasValue && request.AgencyContractId.Value > 0
                ? request.AgencyContractId.Value
                : allocation.AgencyContractId;

            allocation.VehicleInstanceId = request.VehicleInstanceId.HasValue && request.VehicleInstanceId.Value > 0
                ? request.VehicleInstanceId.Value
                : allocation.VehicleInstanceId;

            allocation.AgencyOrderId = request.AgencyOrderId ?? allocation.AgencyOrderId;

            // Có thể cập nhật AllocationDate nếu bạn muốn mỗi lần update đều ghi lại thời gian
            allocation.AllocationDate = DateTime.UtcNow;

            _allocationRepository.Update(allocation);
            await _allocationRepository.SaveChangesAsync();

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
        public async Task<IEnumerable<AllocationResponse>> GetAllocationsAsync()
        {
            var entities = await _allocationRepository.GetAllWithDetailsAsync();
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


        public async Task<IEnumerable<AllocationResponse>> GetByAgencyOrderIdAsync(int agencyOrderId)
        {
            var agencyOrder = await _agencyGrpcClient.GetAgencyOrderByIdAsync(agencyOrderId);
            if (agencyOrder == null)
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng đại lý với ID {agencyOrderId}");

            var entities = await _allocationRepository.GetByAgencyOrderIdAsync(agencyOrderId);
            var result = new List<AllocationResponse>();
            //var result = new List<AllocationResponse>();
            foreach (var entity in entities)
            {
                var agencycontract = await _agencyGrpcClient.GetContractByIdAsync(entity.AgencyContractId);
                var res = MapToResponse(entity);
                res.ContractReply = agencycontract;
                result.Add(res);
            }
                //

            return result;
        }


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
                AgencyOrderId = a.AgencyOrderId,
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
