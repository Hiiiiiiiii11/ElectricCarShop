// In AllocationAPIService/Services/VehicleInstanceGrpcServiceImpl.cs

using Grpc.Core;
using GrpcService;
using AllocationRepository.Repositories; // Your repository namespace

namespace AllocationAPIService.Implement
{
    // Kế thừa từ class base mới được sinh ra từ file .proto
    public class VehicleInstanceGrpcServiceImpl : VehicleInstanceGrpcService.VehicleInstanceGrpcServiceBase
    {
        private readonly IVehicleInstanceRepository _instanceRepository;
        private readonly IVehicleRepository _vehicleRepository;

        public VehicleInstanceGrpcServiceImpl(IVehicleInstanceRepository instanceRepository, IVehicleRepository vehicleRepository)
        {
            _instanceRepository = instanceRepository;
            _vehicleRepository = vehicleRepository;

        }

        // Override lại method GetVehicleInstanceById
        public override async Task<VehicleInstanceReply> GetVehicleInstanceById(GetVehicleInstanceByIdRequest request, ServerCallContext context)
        {
            // Lấy vehicle instance và các thông tin liên quan từ DB
            var instance = await _instanceRepository.GetByIdWithDetailsAsync(request.Id);

            if (instance == null || instance.Vehicle == null || instance.Vehicle.VehicleOption == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy VehicleInstance hoặc thông tin liên quan với ID {request.Id}"));
            }

            // Map dữ liệu từ các model (VehicleInstance, Vehicle, VehicleOption) sang gRPC Reply
            return new VehicleInstanceReply
            {
                // Từ VehicleInstance
                Id = instance.Id,
                Vin = instance.Vin ?? "",
                EngineNumber = instance.EngineNumber ?? "",

                // Từ Vehicle
                VariantName = instance.Vehicle.VariantName ?? "",
                Color = instance.Vehicle.Color ?? "",
                BatteryCapacity = instance.Vehicle.BatteryCapacity ?? "",
                RangeKM = instance.Vehicle.RangeKM,

                // Từ Vehicle.VehicleOption
                ModelName = instance.Vehicle.VehicleOption.ModelName ?? "",
                Description = instance.Vehicle.VehicleOption.Description ?? ""
            };
        }
        public override async Task<VehicleReply> GetVehicleById(GetVehicleByIdRequest request, ServerCallContext context)
        {
            var vehicle = await _vehicleRepository.GetVehicleWithDetailsAsync(request.Id);

            if (vehicle == null || vehicle.VehicleOption == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy Vehicle hoặc thông tin liên quan với ID {request.Id}"));
            }

            return new VehicleReply
            {
                Id = vehicle.Id,
                VariantName = vehicle.VariantName ?? "",
                Color = vehicle.Color ?? "",
                BatteryCapacity = vehicle.BatteryCapacity ?? "",
                RangeKM = vehicle.RangeKM,
                ModelName = vehicle.VehicleOption.ModelName ?? "",
                Description = vehicle.VehicleOption.Description ?? ""
            };
        }
    }
}