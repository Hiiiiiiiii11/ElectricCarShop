// In AllocationAPIService/Services/VehicleInstanceGrpcServiceImpl.cs

using Grpc.Core;
using GrpcService;
using AllocationRepository.Repositories;
using AllocationRepository.Model; // Your repository namespace

namespace AllocationAPIService.Implement
{
    // Kế thừa từ class base mới được sinh ra từ file .proto
    public class VehicleInstanceGrpcServiceImpl : VehicleInstanceGrpcService.VehicleInstanceGrpcServiceBase
    {
        private readonly IVehicleInstanceRepository _instanceRepository;
        private readonly IVehicleRepository _vehicleRepository;
        private readonly IVehicleInstanceRepository _vehicleInstanceRepository;
        private readonly IVehiclePriceRepository _vehiclePriceRepository ;
        private readonly IVehiclePromotionRepository _vehiclePromotionRepository;
        private readonly IAllocationRepository _allocationRepository;

        public VehicleInstanceGrpcServiceImpl(IVehicleInstanceRepository instanceRepository,
            IVehicleRepository vehicleRepository,
            IVehiclePriceRepository vehiclePriceRepository,
            IVehiclePromotionRepository vehiclePromotionRepository,
            IAllocationRepository allocationRepository,
            IVehicleInstanceRepository vehicleInstanceRepository

            )
        {
            _instanceRepository = instanceRepository;
            _vehicleRepository = vehicleRepository;
            _vehiclePriceRepository = vehiclePriceRepository;
            _vehiclePromotionRepository = vehiclePromotionRepository;
            _allocationRepository = allocationRepository;
            _vehicleInstanceRepository = vehicleInstanceRepository;

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
                VehicleId = instance.VehicleId,
                EngineNumber = instance.EngineNumber ?? "",
                Status = instance.Status ?? "",

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
                Description = vehicle.VehicleOption.Description ?? "",
                Features = vehicle.Features ?? ""
            };
        }
        public override async Task GetAllVehicles(GetAllVehicleRequest request, IServerStreamWriter<VehicleReply> responseStream, ServerCallContext context)
        {
            // Giả sử bạn có hàm GetAllWithDetailsAsync (tương tự GetVehicleWithDetailsAsync)
            var vehicles = await _vehicleRepository.GetAllVehicleWithDetailsAsync();

            foreach (var vehicle in vehicles)
            {
                if (vehicle.VehicleOption == null) continue; // Bỏ qua nếu dữ liệu không nhất quán

                await responseStream.WriteAsync(new VehicleReply
                {
                    Id = vehicle.Id,
                    VariantName = vehicle.VariantName ?? "",
                    Color = vehicle.Color ?? "",
                    BatteryCapacity = vehicle.BatteryCapacity ?? "",
                    RangeKM = vehicle.RangeKM,
                    ModelName = vehicle.VehicleOption.ModelName ?? "",
                    Description = vehicle.VehicleOption.Description ?? "",
                    Features = vehicle.Features ?? ""
                });
            }
        }
        public override async Task GetAllVehicleInstances(GetAllVehicleInstanceRequest request, IServerStreamWriter<VehicleInstanceReply> responseStream, ServerCallContext context)
        {
            // Giả sử bạn có hàm GetAllWithDetailsAsync (tương tự GetVehicleWithDetailsAsync)
            var vehicles = await _vehicleInstanceRepository.GetAllWithDetailsAsync();

            foreach (var vehicle in vehicles)
            {
                // Bỏ qua nếu dữ liệu không nhất quán

                await responseStream.WriteAsync(new VehicleInstanceReply
                {
                    Id = vehicle.Id,
                    Vin = vehicle.Vin ?? "",
                    VehicleId = vehicle.VehicleId,
                    EngineNumber = vehicle.EngineNumber ?? "",
                    Status = vehicle.Status ?? "",
                    VariantName = vehicle.Vehicle?.VariantName ?? "",
                    Color = vehicle.Vehicle?.Color ?? "",
                    BatteryCapacity = vehicle.Vehicle?.BatteryCapacity ?? "",
                    RangeKM = vehicle.Vehicle?.RangeKM ?? 0,
                    ModelName = vehicle.Vehicle?.VehicleOption?.ModelName ?? "",
                    Description = vehicle.Vehicle?.VehicleOption?.Description ?? ""

                });
            }
        }
        public override async Task GetAllVehiclePrices(GetAllVehiclePriceRequest request, IServerStreamWriter<VehiclePriceReply> responseStream, ServerCallContext context)
        {
            var prices = await _vehiclePriceRepository.GetAllAsync();

            foreach (var price in prices)
            {
                var reply = new VehiclePriceReply
                {
                    Id = price.Id,
                    VehicleId = price.VehicleId,
                    PriceType = price.PriceType ?? "",
                    PriceAmount = (double)price.PriceAmount, // Chuyển decimal sang double
                    StartDate = price.StartDate.ToString("o"),
                    EndDate = price.EndDate.ToString("o")
                };

                // Xử lý 'oneof' (agencyId có thể null)
                if (price.AgencyId.HasValue)
                {
                    reply.AgencyId = price.AgencyId.Value;
                }

                await responseStream.WriteAsync(reply);
            }
        }
        public override async Task GetAllVehiclePromotions(GetAllVehiclePromotionRequest request, IServerStreamWriter<VehiclePromotionReply> responseStream, ServerCallContext context)
        {
            var promotions = await _vehiclePromotionRepository.GetAllAsync();

            foreach (var promo in promotions)
            {
                await responseStream.WriteAsync(new VehiclePromotionReply
                {
                    Id = promo.Id,
                    VehicleId = promo.VehicleId,
                    PromoName = promo.PromoName ?? "",
                    DiscountAmount = (double)promo.DiscountAmount, // Chuyển decimal sang double
                    StartDate = promo.StartDate.ToString("o"),
                    EndDate = promo.EndDate.ToString("o")
                });
            }
        }
        public override async Task GetAllocations(GetAllocationRequest request, IServerStreamWriter<AllocationReply> responseStream, ServerCallContext context)
        {
            IEnumerable<Allocations> allocations = Enumerable.Empty<Allocations>();

            if (request.Id > 0)
            {
                // Kiểm tra xem có phân phối nào cho VehicleInstanceId này không
                bool exists = (await _allocationRepository.GetByVehicleInstanceIdAsync(request.Id)).Any();

                if (exists)
                {
                    // Có thể stream 1 bản ghi "báo hiệu có"
                    await responseStream.WriteAsync(new AllocationReply
                    {
                        Id = 0, // chỉ là placeholder
                        AgencyContractId = 0,
                        VehicleInstanceId = request.Id,
                        AllocationDate = DateTime.UtcNow.ToString("o") // placeholder, hoặc để null nếu proto cho phép
                    });
                }

            }
        }


    }
}