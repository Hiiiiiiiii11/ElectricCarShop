using AgencyRepository.Model.DTO;
using Share.ShareServices;
using static GrpcService.UserGrpcService;

namespace AgencyService.Services
{
    public class AgencyServiceImpl
    {
        private readonly IAgencyGrpcServiceClient _agencyGrpcClient;
        private readonly IUserGrpcServiceClient _userGrpcClient;

        public AgencyServiceImpl(IAgencyGrpcServiceClient agencyGrpcClient, IUserGrpcServiceClient userGrpcClient)
        {
            _agencyGrpcClient = agencyGrpcClient;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<AgencyWithUsersResponse> GetAgencyWithUsersAsync(int agencyId)
        {
            // 1. Gọi gRPC lấy thông tin agency
            var agency = await _agencyGrpcClient.GetAgencyByIdAsync(agencyId);

            // 2. Gọi gRPC lấy list user theo agency
            var userReplies = await _userGrpcClient.GetUsersByAgencyIdAsync(agencyId);

            // Map UserReply -> UserResponse
            var userResponses = userReplies.Select(u => new UserResponse
            {
                Id = u.Id, // ép kiểu an toàn
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                AvatarUrl = u.AvartarUrl,
                Phone = u.Phone,
                Role = u.Role
            }).ToList();

           // Kết hợp trả về
             return new AgencyWithUsersResponse
            {
                AgencyId = agency.Id,
                AgencyName = agency.AgencyName,
                Address = agency.Address,
                Status = agency.Status,
                Users = userResponses
            };
        }
    }
}
