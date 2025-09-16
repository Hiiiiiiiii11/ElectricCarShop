using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model.DTO
{
    // Request khi tạo role
    public class CreateRoleRequest
    {
        [Required(ErrorMessage = "RoleName is required")]
        public string RoleName { get; set; }
    }
    // Request update role
    public  class UpdateRoleRequest
    {
        [Required(ErrorMessage = "RoleName is required")]
        public string RoleName { get; set; }
    }

    // Response khi lấy role
    public class RoleResponse
    {
        public int Id { get; set; }
        public string RoleName { get; set; }
    }

    // Request khi gán role cho user
    public class AssignUserRoleRequest
    {
        public int UserId { get; set; }
        public List<int> RoleIds { get; set; }
    }
    // Update request cho user
    public class UpdateUserRoleRequest
    {
        public int UserId { get; set; }
        public List<int> NewRoleIds { get; set; }
    }

    // Response cho UserRole
    public class UserRoleResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public List<RoleSupport> Roles { get; set; } = new List<RoleSupport>();
    }
    public class RoleSupport
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
