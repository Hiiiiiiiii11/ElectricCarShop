using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model
{
    public class Users
    {
        public int Id { get; set; }   // EF sẽ tự tăng
        public string UserName { get; set; }
        public string PasswordHash { get; set; }
        public string? FullName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }
        public Roles Role { get; set; }
        public string? Phone { get; set; }
        public string? AvartarUrl { get; set; }
        public string Status { get; set; }
        public DateTime Created_At { get; set; } = DateTime.Now;
        public DateTime Updated_At { get; set; } = DateTime.Now;
        
     

    }
}
