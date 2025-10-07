using AgencyRepository.Repositories;
using Grpc.Core;
using GrpcService;

namespace AgencyService.Services
{
    public class AgencyGrpcServiceImpl : AgencyGrpcService.AgencyGrpcServiceBase
    {
        private readonly IAgencyRepository _agencyRepository;

        public AgencyGrpcServiceImpl(IAgencyRepository agencyRepository)
        {
            _agencyRepository = agencyRepository;
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
    }
}
