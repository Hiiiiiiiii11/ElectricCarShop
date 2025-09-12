using Microsoft.IdentityModel.Tokens;
using Share.Setting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model;
using UserRepository.Model.DTO;
using UserRepository.Repositories;

namespace UserService.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly IAuthenticationRepository _authRepository;
        private readonly JwtSettings _jwtSettings;
        public AuthenticationService(IAuthenticationRepository authRepository, JwtSettings jwtSettings)
        {
            _authRepository = authRepository;
            _jwtSettings = jwtSettings;
        }
        public string GenerateTokenAsync(Users user)
        {
            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("id", user.Id.ToString()),
            new Claim("email", user.Email ?? string.Empty),
            new Claim("phone", user.Phone ?? string.Empty),
            new Claim("username", user.UserName ?? string.Empty),
            new Claim("fullname",user.FullName ?? string.Empty),
            new Claim("role", string.Join(",", user.UserRoles?.Select(ur => ur.Role.RoleName) ?? new List<string>())),
            new Claim("avartarUrl", user.AvartarUrl ?? string.Empty)

        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task<LoginResponse> LoginAsync(UserLoginRequest request)
        {
            var user = await _authRepository.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                throw new KeyNotFoundException("User account isn't exist !");
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid)
                throw new UnauthorizedAccessException("Invalid password!");

            //if (!user.Status)
            //    throw new UnauthorizedAccessException("Account is inactive.");
            var token = GenerateTokenAsync(user);
            return new LoginResponse
            {
                Token = token,
                Expiration = DateTime.UtcNow.AddHours(_jwtSettings.ExpirationHours)
            };
        }
    }
}
