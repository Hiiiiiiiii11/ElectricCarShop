using Grpc.Core;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class VehicleInstanceGrpcServiceClient : IVehicleInstanceGrpcServiceClient
    {
        private readonly VehicleInstanceGrpcService.VehicleInstanceGrpcServiceClient _client;

        public VehicleInstanceGrpcServiceClient(VehicleInstanceGrpcService.VehicleInstanceGrpcServiceClient client)
        {
            _client = client;
        }

        public async Task<VehicleInstanceReply> GetVehicleInstanceByIdAsync(int instanceId)
        {
            var request = new GetVehicleInstanceByIdRequest { Id = instanceId };
            return await _client.GetVehicleInstanceByIdAsync(request);
        }
        public async Task<VehicleReply> GetVehicleByIdAsync(int vehicleId)
        {
            var request = new GetVehicleByIdRequest { Id = vehicleId };
            return await _client.GetVehicleByIdAsync(request);
        }
        public AsyncServerStreamingCall<VehicleReply> GetAllVehicles(GetAllVehicleRequest request)
        {
            return _client.GetAllVehicles(request);
        }
        public AsyncServerStreamingCall<VehicleInstanceReply> GetAllVehicleInstances(GetAllVehicleInstanceRequest request)
        {
            return _client.GetAllVehicleInstances(request);
        }
        public AsyncServerStreamingCall<VehicleInstanceReply> GetAllVehiclePrices(GetAllVehicleInstanceRequest request)
        {
            return _client.GetAllVehicleInstances(request);
        }

        public AsyncServerStreamingCall<VehiclePriceReply> GetAllVehiclePrices(GetAllVehiclePriceRequest request)
        {
            return _client.GetAllVehiclePrices(request);
        }
        

        public AsyncServerStreamingCall<VehiclePromotionReply> GetAllVehiclePromotions(GetAllVehiclePromotionRequest request)
        {
            return _client.GetAllVehiclePromotions(request);
        }
        public AsyncServerStreamingCall<AllocationReply> GetAllocations(GetAllocationRequest request)
        {
            return _client.GetAllocations(request);
        }


    }
}
