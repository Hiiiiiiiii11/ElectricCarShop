using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model.DTO
{
    // Request model khi tạo user
    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
    }

    // Request model khi update user
    public class UpdateUserRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvartarUrl { get; set; }
        public string Status { get; set; }
    }

    // Response model
    public class UserResponse
    {
        public int Id { get; set; }   // EF sẽ tự tăng
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string AvartarUrl { get; set; }
        public string Status { get; set; }
        public DateTime Created_At { get; set; }
        public DateTime Updated_At { get; set; }
    }

    //request model khi login
    public class UserLoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
    //response model khi login
    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
    
}
