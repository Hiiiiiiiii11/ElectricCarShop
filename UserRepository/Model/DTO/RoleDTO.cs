using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model.DTO
{
    // Request khi tạo role
    public class CreateRoleRequest
    {
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
        public int RoleId { get; set; }
    }

    // Response cho UserRole
    public class UserRoleResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }
}
