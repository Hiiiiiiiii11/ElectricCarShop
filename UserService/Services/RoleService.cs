
using UserRepository.Model;
using UserRepository.Model.DTO;
using UserRepository.Repositories;


namespace UserService.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IUserRepository _userRepository;

        public RoleService(IRoleRepository roleRepository, IUserRoleRepository userRoleRepository, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _userRepository = userRepository;
        }

        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
        {
            var existingRole = await _roleRepository.GetByRoleNameAsync(request.RoleName);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Role with name [{request.RoleName}] already exists!");
            }
            var role = new Roles
            {
                RoleName = request.RoleName
            };

            await _roleRepository.AddAsync(role);
            await _roleRepository.SaveChangesAsync();

            return new RoleResponse
            {
                Id = role.Id,
                RoleName = role.RoleName
            };
        }
        public async Task<RoleResponse> UpdateRoleAsync(int id, UpdateRoleRequest request)
        {
            var role =  await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role with ID {id} not found.");
            }
            // Nếu có giá trị mới thì update, còn nếu null/empty thì giữ nguyên cũ
            if (!string.IsNullOrWhiteSpace(request.RoleName))
                role.RoleName = request.RoleName;
            _roleRepository.Update(role);
            await _roleRepository.SaveChangesAsync();
            return new RoleResponse
            {
                Id = role.Id,
                RoleName = role.RoleName
            };
        }

        public async Task<IEnumerable<RoleResponse>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => new RoleResponse
            {
                Id = r.Id,
                RoleName = r.RoleName
            });
        }

        public async Task<RoleResponse> GetRoleByIdAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role with ID {id} not found.");
            }

                return new RoleResponse
            {
                Id = role.Id,
                RoleName = role.RoleName
            };
        }

        public async Task<bool> DeleteRoleAsync(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                throw new KeyNotFoundException($"Role with ID {id} not found.");
            }

                _roleRepository.Remove(role);
            await _roleRepository.SaveChangesAsync();
            return true;
        }

        public async Task<UserRoleResponse> AssignRoleForUser(AssignUserRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if(user == null)
            {
                throw new KeyNotFoundException("User isn't exist");
            }
            var roles = new List<Roles>();
            foreach (var roleId in request.RoleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                {
                    throw new KeyNotFoundException($"Role with ID {roleId} not found.");
                }
                var exists = await _userRoleRepository.FindAsync(ur => ur.UserId == request.UserId && ur.RoleId == roleId);
                if(!exists.Any())
                {
                    await _userRoleRepository.AddAsync(new UserRoles
                    {
                        UserId = request.UserId,
                        RoleId = roleId
                    });
                }
                roles.Add(role);
            }
            await _userRoleRepository.SaveChangesAsync();

            return new UserRoleResponse
            {

                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.Select(r => new RoleSupport
                {
                    RoleId = r.Id,
                    RoleName = r.RoleName
                }).ToList()
            };
        }
        public async Task<UserRoleResponse> UpdateUserRole(UpdateUserRoleRequest request)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException($"UserRole with ID {request.UserId} not found.");

            // Lấy tất cả roles hiện tại của user
            var existingRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == request.UserId);
            if (existingRoles.Any())
            {
                _userRoleRepository.RemoveRange(existingRoles);
            }
            var newRoles = new List<Roles>();
            foreach (var roleId in request.NewRoleIds)
            {
                var role = await _roleRepository.GetByIdAsync(roleId);
                if (role == null)
                    throw new KeyNotFoundException($"Role with ID {roleId} not found.");

                await _userRoleRepository.AddAsync(new UserRoles
                {
                    UserId = request.UserId,
                    RoleId = roleId
                });

                newRoles.Add(role);
            }

            await _userRoleRepository.SaveChangesAsync();

            return new UserRoleResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = newRoles.Select(r => new RoleSupport
                {
                    RoleId = r.Id,
                    RoleName = r.RoleName
                }).ToList()
            };
        }

        public async Task<bool> RemoveUserRolesByUserId(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new KeyNotFoundException($"User with Id {userId} does not exist");

            // Lấy tất cả UserRoles theo UserId
            var userRoles = await _userRoleRepository.FindAsync(ur => ur.UserId == userId);

            if (userRoles == null || !userRoles.Any())
                throw new KeyNotFoundException($"User with Id {userId} does not have any role");

            _userRoleRepository.RemoveRange(userRoles);
            await _userRoleRepository.SaveChangesAsync();
            return true;
        }
    }
}
