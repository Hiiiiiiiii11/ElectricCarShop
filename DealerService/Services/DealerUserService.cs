using DealerRepository.Model;
using DealerRepository.Repositories;
using GrpcService;
using Grpc.Net.Client;
using DealerRepository.Model.DTO;
using Share.ShareServices;

namespace DealerService.Services
{
    public class DealerUserService : IDealerUserService
    {
        private readonly IDealerUserRepository _dealerUserRepository;
        private readonly IUserGrpcServiceClient _userGrpcClient;

        public DealerUserService(
            IDealerUserRepository dealerUserRepository,
            IUserGrpcServiceClient userGrpcClient)
        {
            _dealerUserRepository = dealerUserRepository;
            _userGrpcClient = userGrpcClient;
        }

        public async Task<DealerUserResponse> CreateDealerUserAsync(CreateDealerUserRequest request)
        {
            // Kiểm tra nếu đã tồn tại DealerUser với cùng DealerId và UserId
            var existingDealerUser = await _dealerUserRepository.GetByUserAndDealerAsync(request.UserId, request.DealerId);
            if (existingDealerUser != null)
            {
                throw new InvalidOperationException($"DealerUser with DealerID {request.DealerId} and UserID {request.UserId} already exists.");
            }
            var dealerUser = new DealerUser
            {
                DealerId = request.DealerId,
                UserId = request.UserId,
                Position = request.Position
            };

            await _dealerUserRepository.AddAsync(dealerUser);
            await _dealerUserRepository.SaveChangesAsync();

            var userReply = await _userGrpcClient.GetUserByIdAsync(dealerUser.UserId);

            return new DealerUserResponse
            {
                Id = dealerUser.Id,
                DealerId = dealerUser.DealerId,
                UserId = dealerUser.UserId,
                Position = dealerUser.Position,
                UserName = userReply.UserName,
                Email = userReply.Email,
                AvartarUrl = userReply.AvartarUrl,
                Phone = userReply.Phone
            };
        }

        public async Task RemoveDealerUserAsync(int userId, int dealerId)
        {
            var dealerUser =  await _dealerUserRepository.GetByUserAndDealerAsync(userId, dealerId);
            if (dealerUser == null)
                throw new KeyNotFoundException($"DealerUser with UserID {userId} and DealerID {dealerId} not found.");
            _dealerUserRepository.Remove(dealerUser);
            await _dealerUserRepository.SaveChangesAsync();
        }

        public async Task<DealerUserResponse> GetDealerUserByIdAsync(int userId)
        {
            var dealerUser = await _dealerUserRepository.GetDealerUserByUserId(userId);
            if (dealerUser == null)
                throw new KeyNotFoundException($"DealerUser with ID {userId} not found.");

            // Gọi gRPC sang UserService
            var userReply = await _userGrpcClient.GetUserByIdAsync(dealerUser.UserId);

            return new DealerUserResponse
            {
                Id = dealerUser.Id,
                DealerId = dealerUser.DealerId,
                UserId = dealerUser.UserId,
                Position = dealerUser.Position,
                UserName = userReply.UserName,
                Email = userReply.Email,
                AvartarUrl = userReply.AvartarUrl,
                Phone = userReply.Phone
            };
        }

        public async Task<IEnumerable<DealerUserResponse>> GetDealerUsersByDealerIdAsync(int dealerId)
        {
            var dealerUsers =  await _dealerUserRepository.GetUsersByDealerIdAsync(dealerId);
            var result = new List<DealerUserResponse>();
            foreach (var  du in dealerUsers)
            {
                var userReply = await _userGrpcClient.GetUserByIdAsync(du.UserId);

                result.Add(new DealerUserResponse
                {
                    Id = du.Id,
                    DealerId = du.DealerId,
                    UserId = du.UserId,
                    Position = du.Position,
                    UserName = userReply.UserName,
                    Email = userReply.Email,
                    AvartarUrl = userReply.AvartarUrl,
                    Phone = userReply.Phone
                });
            }
            return result;
        }
    }
}
