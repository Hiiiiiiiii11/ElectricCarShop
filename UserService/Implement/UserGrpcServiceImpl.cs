using Grpc.Core;
using GrpcService;
using System.Linq;
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
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new RpcException(new Status(StatusCode.NotFound, $"User {request.UserId} not found"));

            return new UserReply
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                AvartarUrl = user.AvartarUrl ?? "",
                Phone = user.Phone ?? "",
                Role = user.Role?.RoleName ?? ""
            };
        }

        public override async Task<UsersReply> GetUsersByAgencyId(GetUsersByAgencyIdRequest request, ServerCallContext context)
        {
            var users = await _userRepository.GetAllAsync();
            var filtered = users.Where(u => u.AgencyId == request.AgencyId).ToList();

            var reply = new UsersReply();
            reply.Users.AddRange(filtered.Select(u => new UserReply
            {
                Id = u.Id,
                UserName = u.UserName,
                FullName = u.FullName,
                Email = u.Email,
                AvartarUrl = u.AvartarUrl ?? "",
                Phone = u.Phone ?? "",
                Role = u.Role?.RoleName ?? ""
            }));

            return reply;
        }
        public override async Task<AssignUserToAgencyReply> AssignUserToAgency(AssignUserToAgencyRequest request, ServerCallContext context)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
            {
                return new AssignUserToAgencyReply { Success = false};
            }

            user.AgencyId = request.AgencyId;  // thêm field AgencyId vào bảng Users
            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return new AssignUserToAgencyReply { Success = true};
        }

    }
}
