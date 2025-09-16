using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserRepository.Model.DTO;

namespace UserService.Services
{
    public interface IEmailVerificationService
    {
        Task SendVerificationCodeAsync(string email);
        Task<bool> VerifyCodeAsync(VerifyOTPRequest request);
    }
}
