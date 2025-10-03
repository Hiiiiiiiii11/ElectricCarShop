using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Share.ShareServices
{
    public class UserGrpcServiceClient : IUserGrpcServiceClient
    {
        private readonly UserGrpcService.UserGrpcServiceClient _client;

        public UserGrpcServiceClient(UserGrpcService.UserGrpcServiceClient client)
        {
            _client = client;
        }

        public async Task<UserReply> GetUserByIdAsync(int userId)
        {
            return await _client.GetUserByIdAsync(new GetUserByIdRequest { UserId = userId });
        }
        public async Task<IEnumerable<UserReply>> GetUsersByAgencyIdAsync(int agencyId)
        {
            var response = await _client.GetUsersByAgencyIdAsync(new GetUsersByAgencyIdRequest
            {
                AgencyId = agencyId
            });

            return response.Users;
        }
        public async Task<bool> AssignUserToAgencyAsync(int userId, int agencyId)
        {
            var request = new AssignUserToAgencyRequest
            {
                UserId = userId,
                AgencyId = agencyId
            };

            var response = await _client.AssignUserToAgencyAsync(request);
            return response.Success;
        }
    }
}
