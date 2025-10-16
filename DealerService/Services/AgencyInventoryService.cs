using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using GrpcService;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyInventoryService : IAgencyInventoryService
    {
        private readonly IAgencyInventoryRepository _agencyInventoryRepository;
        private readonly IVehicleInstanceGrpcServiceClient _vehicleGrpcClient;
        public AgencyInventoryService(IAgencyInventoryRepository AgencyInventoryRepository , IVehicleInstanceGrpcServiceClient vehicleInstanceGrpcServiceClient)
        {
            _agencyInventoryRepository = AgencyInventoryRepository;
            _vehicleGrpcClient = vehicleInstanceGrpcServiceClient;
        }

        public async Task<AgencyInventoryResponse> CreateAgencyInventoryAsync(int AgencyId, CreateAgencyInventoryRequest request)
        {
            var newInventory = new AgencyInventory
            {
                AgencyId = AgencyId,
                VehicleInstanceId = request.VehicleInstanceId,
            };
            await _agencyInventoryRepository.AddAsync(newInventory);
            await _agencyInventoryRepository.SaveChangesAsync();
            return MapToResponse(newInventory);
        }

        public async Task<IEnumerable<AgencyInventoryResponse>> GetInventoriesByAgencyIdAsync(int agencyId)
        {
            // 2. Lấy dữ liệu từ DB và làm giàu bằng gRPC
            var inventories = await _agencyInventoryRepository.GetInventoriesByAgencyIdAsync(agencyId);
            var responseList = new List<AgencyInventoryResponse>();

            foreach (var inv in inventories)
            {
                VehicleInstanceReply vehicleDetails = null;
                try
                {
                    // Gọi gRPC để lấy thông tin chi tiết của xe
                    vehicleDetails = await _vehicleGrpcClient.GetVehicleInstanceByIdAsync(inv.VehicleInstanceId);
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu cần, tạm thời bỏ qua để không làm hỏng toàn bộ request
                    Console.WriteLine($"Error fetching vehicle details for ID {inv.VehicleInstanceId}: {ex.Message}");
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

        public async Task<AgencyInventoryResponse?> GetInventoryAsync(int agencyId, int vehicleInstanceId)
        {
            var inventory = await _agencyInventoryRepository.GetInventoryAsync(agencyId, vehicleInstanceId);
            if (inventory == null)
            {
                // Trả về null hoặc throw Exception tùy theo logic bạn muốn
                return null;
            }

            // Gọi gRPC để lấy thông tin chi tiết xe
            var vehicleDetails = await _vehicleGrpcClient.GetVehicleInstanceByIdAsync(inventory.VehicleInstanceId);

            return new AgencyInventoryResponse
            {
                Id = inventory.Id,
                AgencyId = inventory.AgencyId,
                VehicleInstanceId = inventory.VehicleInstanceId,
                Agency = MapAgencyToResponse(inventory.Agency),
                VehicleDetails = vehicleDetails
            };
        }



        public Task RemoveInventoryItemAsync(int AgencyId, int variantId)
        {
            var inventory = _agencyInventoryRepository.GetInventoryAsync(AgencyId, variantId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            return _agencyInventoryRepository.RemoveInventoryItemAsync(AgencyId, variantId);
        }



        public async Task<AgencyInventoryResponse> UpdateInventoryAsync(int AgencyId, UpdateAgencyInventoryRequest request)
        {
            var inventory = await _agencyInventoryRepository.GetInventoryAsync(AgencyId, request.VehicleInstanceId);
            if (inventory == null)
            {
                throw new Exception("Inventory item not found.");
            }
            inventory.VehicleInstanceId = request.VehicleInstanceId;
            _agencyInventoryRepository.Update(inventory);
            await _agencyInventoryRepository.SaveChangesAsync();
            return MapToResponse(inventory);
        }
        public AgencyInventoryResponse MapToResponse(AgencyInventory inventory)
        {
            return new AgencyInventoryResponse
            {
                Id = inventory.Id,
                AgencyId = inventory.AgencyId,
                VehicleInstanceId = inventory.VehicleInstanceId,
                Agency = inventory.Agency == null ? null : new AgencyResponseForTarget
                {
                    Id = inventory.Agency.Id,
                    AgencyName = inventory.Agency.AgencyName,
                    Email = inventory.Agency.Email,
                    Phone = inventory.Agency.Phone,
                    Address = inventory.Agency.Address,
                    Status = inventory.Agency.Status,
                }
            };
        }
        private AgencyResponseForTarget? MapAgencyToResponse(Agency agency)
        {
            if (agency == null) return null;
            return new AgencyResponseForTarget
            {
                Id = agency.Id,
                AgencyName = agency.AgencyName,
                Email = agency.Email,
                Phone = agency.Phone,
                Address = agency.Address,
                Status = agency.Status,
            };
        }
    }
}
