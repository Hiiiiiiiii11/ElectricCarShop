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
            return await _client.GetUserByIdAsync(new GetUserByIdRequest { Id = userId.ToString() });
        }
    }
}
