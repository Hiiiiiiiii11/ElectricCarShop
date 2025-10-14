using AllocationRepository.Repositories;
using Grpc.Core;
using GrpcService;

namespace AllocationAPIService.Services
{
    // Kế thừa từ class base được sinh ra tự động bởi gRPC
    public class VehicleGrpcServiceImpl : VehicleGrpcService.VehicleGrpcServiceBase
    {
        private readonly IVehicleRepository _vehicleRepository;

        // Inject repository vào
        public VehicleGrpcServiceImpl(IVehicleRepository vehicleRepository)
        {
            _vehicleRepository = vehicleRepository;
        }

        // Override lại method GetVehicleById từ file .proto
        public override async Task<VehicleReply> GetVehicleById(GetVehicleByIdRequest request, ServerCallContext context)
        {
            // Lấy vehicle từ database
            var vehicle = await _vehicleRepository.GetByIdAsync(request.Id);

            // Xử lý trường hợp không tìm thấy
            if (vehicle == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy xe với ID {request.Id}"));
            }

            // Map từ model sang đối tượng Reply của gRPC và trả về
            return new VehicleReply
            {
                Id = vehicle.Id,
                VariantName = vehicle.VariantName ?? "",
                Color = vehicle.Color ?? "",
                BatteryCapacity = vehicle.BatteryCapacity ?? "",
                RangeKM = vehicle.RangeKM,
                Status = vehicle.Status ?? ""
            };
        }
    }
}
