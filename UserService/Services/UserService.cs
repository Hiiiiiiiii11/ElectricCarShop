
using UserRepository.Model;
using UserRepository.Model.DTO;
using UserRepository.Repositories;

namespace UserService.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthenticationRepository _authRepository;

        public UserService(IUserRepository userRepository, IAuthenticationRepository authRepository)
        {
            _userRepository = userRepository;
            _authRepository = authRepository;
        }

        public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
        {
            var existingUser = await _authRepository.GetUserByEmailAsync(request.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"User with email {request.Email} already exists!");
            }
            string harshPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = new Users
            {
                UserName = request.UserName,
                PasswordHash = harshPassword,
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                AvartarUrl = request.AvartarUrl,
                Status = "Active",
                Created_At = DateTime.UtcNow,
                Updated_At = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task<UserResponse> UpdateUserAsync(int id, UpdateUserRequest request)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            user.FullName = request.FullName;
            user.Email = request.Email;
            user.Phone = request.Phone;
            user.AvartarUrl = request.AvartarUrl;
            user.Status = request.Status;
            user.Updated_At = DateTime.UtcNow;

            _userRepository.Update(user);
            await _userRepository.SaveChangesAsync();

            return MapToResponse(user);
        }

        public async Task<UserResponse> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }
            return MapToResponse(user);
        }

        public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToResponse);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID {id} not found.");
            }

            _userRepository.Remove(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        //mapping user to user response
        private UserResponse MapToResponse(Users user)
        {
            return new UserResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Phone = user.Phone,
                AvartarUrl = user.AvartarUrl,
                Status = user.Status,
                Created_At = user.Created_At,
                Updated_At = user.Updated_At
            };
        }
    }
}
