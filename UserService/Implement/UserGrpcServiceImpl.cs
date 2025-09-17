using Grpc.Core;
using GrpcService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Repositories;

namespace UserService.Implement
{
    public class UserGrpcServiceImpl : UserGrpcService.UserGrpcServiceBase
    {
        private readonly IUserRepository _userRepository;

        public UserGrpcServiceImpl(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override async Task<UserReply> GetUserById(GetUserByIdRequest request, ServerCallContext context)
        {
            if (!int.TryParse(request.Id, out var userId))
                throw new RpcException(new Status(StatusCode.InvalidArgument, "Invalid user ID"));

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"User {userId} not found"));

            return new UserReply
            {
                UserName = user.UserName,
                Email = user.Email,
                AvartarUrl = user.AvartarUrl ?? "",
                Phone = user.Phone ?? ""
            };
        }
    }
}
