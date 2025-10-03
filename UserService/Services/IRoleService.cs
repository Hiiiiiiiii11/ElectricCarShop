using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model.DTO;

namespace UserService.Services
{
    public interface IRoleService
    {
        Task<RoleResponse> CreateRoleAsync(CreateRoleRequest request);
        Task<IEnumerable<RoleResponse>> GetAllRolesAsync();
        Task<RoleResponse> GetRoleByIdAsync(int id);
        Task<RoleResponse> UpdateRoleAsync(int id, UpdateRoleRequest request);
        Task<bool> DeleteRoleAsync(int id);
        //Task<bool> RemoveUserRolesByUserId(int userId);
        

    }
}
