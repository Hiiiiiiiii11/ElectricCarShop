using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserRepository.Model.DTO
{
    // Request model khi tạo user
    public class CreateUserRequest
    {
        [Required(ErrorMessage = "UserName is required")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Required(ErrorMessage = "FullName is required")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "RoleId is required")]
        public int RoleId { get; set; }

        [Required(ErrorMessage = "Phone is required")]
        public string Phone { get; set; }
        public IFormFile? AvartarFile { get; set; }
    }

    // Request model khi update user
    public class UpdateUserRequest
    {
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public IFormFile? AvartarFile { get; set; }
        public string? Status { get; set; }
        public int? RoleId { get; set; }
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
        public RoleResponse Role { get; set; }
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


