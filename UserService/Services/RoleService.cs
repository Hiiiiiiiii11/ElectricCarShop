
using UserRepository.Model;
using UserRepository.Model.DTO;
using UserRepository.Repositories;


namespace UserService.Services
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRepository _userRepository;

        public RoleService(IRoleRepository roleRepository, IUserRepository userRepository)
        {
            _roleRepository = roleRepository;
            _userRepository = userRepository;
        }

        public async Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request)
        {
            var existingRole = await _roleRepository.GetByRoleNameAsync(request.RoleName);
            if (existingRole)
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


    }
}
