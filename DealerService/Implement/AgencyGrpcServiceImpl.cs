using AgencyRepository.Repositories;
using Grpc.Core;
using GrpcService;

namespace AgencyService.Implement
{
    public class AgencyGrpcServiceImpl : AgencyGrpcService.AgencyGrpcServiceBase
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly IAgencyContractRepository _contractRepo;
        private readonly IAgencyTargetRepository _targetRepo;
        private readonly ITestDriveRepository _testDriveRepo;
        private readonly IAgencyOrderRepository _agencyOrderRepo;

        public AgencyGrpcServiceImpl(
            IAgencyRepository agencyRepository,
            IAgencyContractRepository contractRepo,
            IAgencyTargetRepository targetRepo,      
            ITestDriveRepository testDriveRepo,      
            IAgencyOrderRepository agencyOrderRepo)  
        {
            _agencyRepository = agencyRepository;
            _contractRepo = contractRepo;
            _targetRepo = targetRepo;
            _testDriveRepo = testDriveRepo;
            _agencyOrderRepo = agencyOrderRepo;
        }

        // Đây là method gRPC thực sự implement từ file .proto
        public override async Task<AgencyReply> GetAgencyById(GetAgencyByIdRequest request, ServerCallContext context)
        {
            var agency = await _agencyRepository.GetByIdAsync(request.Id);
            if (agency == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy đại lý với ID {request.Id}"));
            }

            return new AgencyReply
            {
                Id = agency.Id,
                AgencyName = agency.AgencyName ?? "",
                Address = agency.Address ?? "",
                Phone = agency.Phone ?? "",
                Email = agency.Email ?? "",
                Status = agency.Status ?? ""
            };
        }
        public override async Task<AgencyContractReply> GetAgencyContractById(GetAgencyContractByIdRequest request, ServerCallContext context)
        {
            var contract = await _contractRepo.GetByIdAsync(request.Id);
            if (contract == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy hợp đồng {request.Id}"));

            var agency = await _agencyRepository.GetByIdAsync(contract.AgencyId);

            return new AgencyContractReply
            {
                Id = contract.Id,
                AgencyId = contract.AgencyId,
                ContractNumber = contract.ContractNumber ?? "",
                Status = contract.Status ?? "",
                AgencyName = agency?.AgencyName ?? "",
                AgencyEmail = agency?.Email ?? ""
            };
        }
        public override async Task GetAllAgencies(GetAllAgencyRequest request, IServerStreamWriter<AgencyReply> responseStream, ServerCallContext context)
        {
            var agencies = await _agencyRepository.GetAllAsync(); // Lấy tất cả

            foreach (var agency in agencies)
            {
                await responseStream.WriteAsync(new AgencyReply
                {
                    Id = agency.Id,
                    AgencyName = agency.AgencyName ?? "",
                    Address = agency.Address ?? "",
                    Phone = agency.Phone ?? "",
                    Email = agency.Email ?? "",
                    Status = agency.Status ?? "",
                    Location = agency.Location ?? ""
                });
            }
        }
        public override async Task GetAllAgencyTargets(GetAllAgencyTargetRequest request, IServerStreamWriter<AgencyTargetReply> responseStream, ServerCallContext context)
        {
            var targets = await _targetRepo.GetAllAsync(); // Lấy tất cả

            foreach (var target in targets)
            {
                await responseStream.WriteAsync(new AgencyTargetReply
                {
                    Id = target.Id,
                    AgencyId = target.AgencyId,
                    TargetYear = target.TargetYear,
                    TargetMonth = target.TargetMonth,
                    TargetSales = target.TargetSales,
                    AchievedSales = target.AchievedSales
                });
            }
        }
        public override async Task GetAllTestDrives(GetAllTestDriveRequest request, IServerStreamWriter<TestDriveReply> responseStream, ServerCallContext context)
        {
            var testDrives = await _testDriveRepo.GetAllAsync(); // Lấy tất cả

            foreach (var drive in testDrives)
            {
                await responseStream.WriteAsync(new TestDriveReply
                {
                    Id = drive.Id,
                    AgencyId = drive.AgencyId,
                    VehicleInstanceId = drive.VehicleInstanceId,
                    CustomerId = drive.CustomerId,
                    // Chuyển DateTime sang string ISO 8601 (chuẩn)
                    AppointmentDate = drive.AppointmentDate.ToString(),
                    Status = drive.Status ?? ""
                });
            }
        }
        public override async Task GetAllAgencyOrders(GetAllAgencyOrderRequest request, IServerStreamWriter<AgencyOrderReply> responseStream, ServerCallContext context)
        {
            var orders = await _agencyOrderRepo.GetAllAsync(); // Lấy tất cả

            foreach (var order in orders)
            {
                await responseStream.WriteAsync(new AgencyOrderReply
                {
                    Id = order.Id,
                    AgencyId = order.AgencyId,
                    VehicleId = order.VehicleId,
                    Quantity = order.Quantity,
                    Status = order.Status ?? ""
                });
            }
        }
        public override async Task<AgencyOrderReply> GetAgencyOrderById(GetAgencyOrderByIdRequest request,ServerCallContext context)
        {
            var order = await _agencyOrderRepo.GetByIdAsync(request.Id);

            if (order == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, $"Không tìm thấy đơn hàng đại lý với ID {request.Id}"));
            }

            return new AgencyOrderReply
            {
                Id = order.Id,
                AgencyId = order.AgencyId,
                VehicleId = order.VehicleId,
                Quantity = order.Quantity,
                Status = order.Status ?? ""
            };
        }
    }
}
