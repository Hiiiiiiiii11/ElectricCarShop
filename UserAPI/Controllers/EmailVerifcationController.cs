using Microsoft.AspNetCore.Mvc;
using UserRepository.Model.DTO;
using UserService.Services;

namespace UserAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmailVerifcationController : Controller
    {
        private readonly IEmailVerificationService _emailVerificationService;
        public EmailVerifcationController(IEmailVerificationService emailVerificationService)
        {
            _emailVerificationService = emailVerificationService;
        }
        [HttpPost("send-otp")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string email)
        {
            try
            {
                await _emailVerificationService.SendVerificationCodeAsync(email);
                return Ok(new { message = "Verification code sent successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromQuery]VerifyOTPRequest request)
        {
            try
            {
                var isValid = await _emailVerificationService.VerifyCodeAsync(request);
                if (isValid)
                {
                    return Ok(new { message = "Email verified successfully." });
                }
                else
                {
                    return BadRequest(new { message = "Invalid or expired verification code." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
