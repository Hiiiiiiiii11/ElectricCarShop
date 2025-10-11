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
    public class UpdateRoleRequest
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

  
}
