using Grpc.Core;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public interface IVehicleInstanceGrpcServiceClient
    {
        Task<VehicleInstanceReply> GetVehicleInstanceByIdAsync(int instanceId);
        Task<VehicleReply> GetVehicleByIdAsync(int vehicleId);
        AsyncServerStreamingCall<VehicleReply> GetAllVehicles(GetAllVehicleRequest request);
        AsyncServerStreamingCall<VehicleInstanceReply> GetAllVehicleInstances(GetAllVehicleInstanceRequest request);
        AsyncServerStreamingCall<VehiclePriceReply> GetAllVehiclePrices(GetAllVehiclePriceRequest request);
        AsyncServerStreamingCall<VehiclePromotionReply> GetAllVehiclePromotions(GetAllVehiclePromotionRequest request);
        AsyncServerStreamingCall<AllocationReply> GetAllocations(GetAllocationRequest request);
    }
}

