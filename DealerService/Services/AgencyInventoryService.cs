using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using Grpc.Core;
using GrpcService;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyInventoryService : IAgencyInventoryService
    {
        private readonly IAgencyInventoryRepository _agencyInventoryRepository;
        private readonly IVehicleInstanceGrpcServiceClient _vehicleGrpcClient;

        public AgencyInventoryService(
            IAgencyInventoryRepository agencyInventoryRepository,
            IVehicleInstanceGrpcServiceClient vehicleInstanceGrpcServiceClient)
        {
            _agencyInventoryRepository = agencyInventoryRepository;
            _vehicleGrpcClient = vehicleInstanceGrpcServiceClient;
        }

        // =================== CREATE ===================
        public async Task<AgencyInventoryResponse> CreateAgencyInventoryAsync(int agencyId, CreateAgencyInventoryRequest request)
        {
            var existingInventory = await _agencyInventoryRepository.GetInventoryAsync(agencyId, request.VehicleInstanceId);
            if (existingInventory != null)
                throw new Exception($"Xe (Instance ID={request.VehicleInstanceId}) đã có trong kho của Agency {agencyId}.");
            // 🧩 1. Gọi gRPC để lấy danh sách allocations của vehicleInstanceId
            var allocationStream = _vehicleGrpcClient.GetAllocations(new GetAllocationRequest { Id = request.VehicleInstanceId });

            bool isAllocated = false;
            await foreach (var allocation in allocationStream.ResponseStream.ReadAllAsync())
            {
                if (allocation.VehicleInstanceId == request.VehicleInstanceId)
                {
                    isAllocated = true;
                    break;
                }
            }

            if (!isAllocated)
            {
                throw new Exception($"Xe (Instance ID={request.VehicleInstanceId}) chưa được phân phối — không thể thêm vào kho.");
            }

            // 🧩 2. Nếu xe đã có phân phối → thêm vào kho
            var newInventory = new AgencyInventory
            {
                AgencyId = agencyId,
                VehicleInstanceId = request.VehicleInstanceId,
            };

            await _agencyInventoryRepository.AddAsync(newInventory);
            await _agencyInventoryRepository.SaveChangesAsync();

            return MapInventoryToResponse(newInventory);
        }

        // =================== GET ALL ===================
        public async Task<IEnumerable<AgencyInventoryResponse>> GetInventoriesByAgencyIdAsync(int agencyId)
        {
            var inventories = await _agencyInventoryRepository.GetInventoriesByAgencyIdAsync(agencyId);
            var responseList = new List<AgencyInventoryResponse>();

            foreach (var inv in inventories)
            {
                VehicleInstanceReply? vehicleDetails = null;

                try
                {
                    // Gọi gRPC để lấy thông tin chi tiết của xe
                    vehicleDetails = await _vehicleGrpcClient.GetVehicleInstanceByIdAsync(inv.VehicleInstanceId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARN] Không thể lấy thông tin xe ID={inv.VehicleInstanceId}: {ex.Message}");
                }

                responseList.Add(new AgencyInventoryResponse
                {
                    Id = inv.Id,
                    AgencyId = inv.AgencyId,
                    VehicleInstanceId = inv.VehicleInstanceId,
                    Agency = MapAgencyToResponse(inv.Agency),
                    VehicleDetails = vehicleDetails
                });
            }

            return responseList;
        }

        // =================== GET BY ID ===================
        public async Task<AgencyInventoryResponse?> GetInventoryAsync(int agencyId, int vehicleInstanceId)
        {


            var inventory = await _agencyInventoryRepository.GetInventoryAsync(agencyId, vehicleInstanceId);
            if (inventory == null)
                return null;

            VehicleInstanceReply? vehicleDetails = null;
            try
            {
                vehicleDetails = await _vehicleGrpcClient.GetVehicleInstanceByIdAsync(inventory.VehicleInstanceId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARN] Lỗi khi lấy chi tiết xe {inventory.VehicleInstanceId}: {ex.Message}");
            }

            return new AgencyInventoryResponse
            {
                Id = inventory.Id,
                AgencyId = inventory.AgencyId,
                VehicleInstanceId = inventory.VehicleInstanceId,
                Agency = MapAgencyToResponse(inventory.Agency),
                VehicleDetails = vehicleDetails
            };
        }

        // =================== DELETE ===================
        public async Task RemoveInventoryItemAsync(int agencyId, int vehicleInstanceId)
        {
            var inventory = await _agencyInventoryRepository.GetInventoryAsync(agencyId, vehicleInstanceId);
            if (inventory == null)
                throw new Exception("Inventory item not found.");

            // ⚠️ Bạn quên await ở đây → context bị dispose sớm → lỗi connection closed
            await _agencyInventoryRepository.RemoveInventoryItemAsync(agencyId, vehicleInstanceId);
        }

        // =================== UPDATE ===================
        public async Task<AgencyInventoryResponse> UpdateInventoryAsync(int agencyId, UpdateAgencyInventoryRequest request)
        {
            var inventory = await _agencyInventoryRepository.GetInventoryAsync(agencyId, request.VehicleInstanceId);
            if (inventory == null)
                throw new Exception("Inventory item not found.");

            if (inventory.VehicleInstanceId != request.VehicleInstanceId)
            {
                var exists = await _agencyInventoryRepository.GetInventoryAsync(agencyId, request.VehicleInstanceId);
                if (exists != null)
                    throw new Exception($"Xe (Instance ID={request.VehicleInstanceId}) đã tồn tại trong kho của Agency {agencyId}.");

                inventory.VehicleInstanceId = request.VehicleInstanceId;
            }

            _agencyInventoryRepository.Update(inventory);
            await _agencyInventoryRepository.SaveChangesAsync();

            return MapInventoryToResponse(inventory);
        }

        // =================== MAPPERS ===================
        private static AgencyInventoryResponse MapInventoryToResponse(AgencyInventory inventory)
        {
            return new AgencyInventoryResponse
            {
                Id = inventory.Id,
                AgencyId = inventory.AgencyId,
                VehicleInstanceId = inventory.VehicleInstanceId,
                Agency = MapAgencyToResponse(inventory.Agency)
            };
        }

        private static AgencyResponse MapAgencyToResponse(Agency? agency)
        {
            if (agency == null) return null;

            return new AgencyResponse
            {
                Id = agency.Id,
                AgencyName = agency.AgencyName,
                Email = agency.Email,
                Phone = agency.Phone,
                Address = agency.Address,
                Status = agency.Status
            };
        }


    }
}
