using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyRepository.Repositories;
using GrpcService;
using Share.ShareServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AgencyService.Services
{
    public class AgencyOrderService : IAgencyOrderService
    {
        private readonly IAgencyOrderRepository _agencyOrderRepository;
        private readonly IVehicleInstanceGrpcServiceClient _vehicleGrpcClient;

        public AgencyOrderService(
            IAgencyOrderRepository agencyOrderRepository,
            IVehicleInstanceGrpcServiceClient vehicleGrpcClient)
        {
            _agencyOrderRepository = agencyOrderRepository;
            _vehicleGrpcClient = vehicleGrpcClient;
        }

        // ===== CREATE =====
        public async Task<AgencyOrderResponse> CreateAsync(CreateAgencyOrderRequest request)
        {
            // ✅ Kiểm tra xe qua gRPC
            var vehicle = await _vehicleGrpcClient.GetVehicleByIdAsync(request.VehicleId);
            if (vehicle == null)
                throw new Exception($"Không tìm thấy xe với ID {request.VehicleId}.");

            var entity = new AgencyOrder
            {
                AgencyId = request.AgencyId,
                AgencyContractId = request.AgencyContractId,
                VehicleId = request.VehicleId,
                Quantity = request.Quantity,
                Status = "Pending",
            };

            await _agencyOrderRepository.AddAsync(entity);
            await _agencyOrderRepository.SaveChangesAsync();

            var response = MapToResponse(entity);
            response.VehicleReply = vehicle;
            return response;
        }

        // ===== UPDATE =====
        public async Task<AgencyOrderResponse> UpdateAsync(UpdateAgencyOrderRequest request)
        {
            var entity = await _agencyOrderRepository.GetByIdAsync(request.Id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng với ID {request.Id}.");

            if (request.VehicleId.HasValue)
                entity.VehicleId = request.VehicleId.Value;
            if (request.Quantity.HasValue)
                entity.Quantity = request.Quantity.Value;
            if (!string.IsNullOrWhiteSpace(request.Status))
                entity.Status = request.Status;

            _agencyOrderRepository.Update(entity);
            await _agencyOrderRepository.SaveChangesAsync();

            var vehicle = await _vehicleGrpcClient.GetVehicleByIdAsync(entity.VehicleId);

            var response = MapToResponse(entity);
            response.VehicleReply = vehicle;

            return response;
        }

        // ===== DELETE =====
        public async Task<bool> DeleteAsync(int id)
        {
            var order = await _agencyOrderRepository.GetByIdAsync(id);
            if (order == null)
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng với ID {id}.");

            _agencyOrderRepository.Remove(order);
            await _agencyOrderRepository.SaveChangesAsync();
            return true;
        }

        // ===== GET ALL =====
        public async Task<IEnumerable<AgencyOrderResponse>> GetAllAsync()
        {
            var orders = await _agencyOrderRepository.GetAllAsync();
            var result = new List<AgencyOrderResponse>();

            foreach (var o in orders)
            {
                var response = MapToResponse(o);
                var vehicle = await _vehicleGrpcClient.GetVehicleByIdAsync(o.VehicleId);
                response.VehicleReply = vehicle;
                result.Add(response);
            }

            return result;
        }

        // ===== GET BY ID =====
        public async Task<AgencyOrderResponse> GetByIdAsync(int id)
        {
            var entity = await _agencyOrderRepository.GetByIdAsync(id);
            if (entity == null)
                throw new KeyNotFoundException($"Không tìm thấy đơn hàng với ID {id}.");

            var vehicle = await _vehicleGrpcClient.GetVehicleByIdAsync(entity.VehicleId);

            var response = MapToResponse(entity);
            response.VehicleReply = vehicle;

            return response;
        }
        public async Task<IEnumerable<AgencyOrderResponse>> GetByAgencyAsync(int agencyId)
        {
            var orders = await _agencyOrderRepository.GetAllAsync();
            var filtered = orders.Where(o => o.AgencyId == agencyId);
            var result = new List<AgencyOrderResponse>();

            foreach (var o in filtered)
            {
                var response = MapToResponse(o);
                var vehicle = await _vehicleGrpcClient.GetVehicleByIdAsync(o.VehicleId);
                response.VehicleReply = vehicle;
                result.Add(response);
            }

            return result;
        }

        public async Task<IEnumerable<AgencyOrderResponse>> GetByContractAsync(int contractId)
        {
            var orders = await _agencyOrderRepository.GetAllAsync();
            var filtered = orders.Where(o => o.AgencyContractId == contractId);
            var result = new List<AgencyOrderResponse>();

            foreach (var o in filtered)
            {
                var response = MapToResponse(o);
                var vehicle = await _vehicleGrpcClient.GetVehicleByIdAsync(o.VehicleId);
                response.VehicleReply = vehicle;
                result.Add(response);
            }

            return result;
        }


        // ===== MAP =====
        private AgencyOrderResponse MapToResponse(AgencyOrder order)
        {
            return new AgencyOrderResponse
            {
                Id = order.Id,
                AgencyId = order.AgencyId,
                AgencyContractId = order.AgencyContractId,
                VehicleId = order.VehicleId,
                Quantity = order.Quantity,
                Status = order.Status,
                CreatedAt = DateTime.UtcNow // Hoặc thêm trường CreateAt vào entity
            };
        }

    }
}
