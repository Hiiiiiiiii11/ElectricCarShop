using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class VehicleGrpcServiceClient : IVehicleGrpcServiceClient
    {
        private readonly VehicleGrpcService.VehicleGrpcServiceClient _client;

        // Inject gRPC client được sinh ra tự động
        public VehicleGrpcServiceClient(VehicleGrpcService.VehicleGrpcServiceClient client)
        {
            _client = client;
        }

        // Đóng gói lại lời gọi gRPC
        public async Task<VehicleReply> GetVehicleByIdAsync(int vehicleId)
        {
            var request = new GetVehicleByIdRequest { Id = vehicleId };
            return await _client.GetVehicleByIdAsync(request);
        }
    }
}
