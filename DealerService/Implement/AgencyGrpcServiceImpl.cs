using AgencyRepository.Repositories;
using Grpc.Core;
using GrpcService;

namespace AgencyService.Implement
{
    public class AgencyGrpcServiceImpl : AgencyGrpcService.AgencyGrpcServiceBase
    {
        private readonly IAgencyRepository _agencyRepository;
        private readonly IAgencyContractRepository _contractRepo;

        public AgencyGrpcServiceImpl(IAgencyRepository agencyRepository, IAgencyContractRepository contractRepo)
        {
            _agencyRepository = agencyRepository;
            _contractRepo = contractRepo;
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
    }
}
