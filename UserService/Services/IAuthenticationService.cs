using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model;
using UserRepository.Model.DTO;

namespace UserService.Services
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(UserLoginRequest request);
        string GenerateTokenAsync(Users user);
    }
}
