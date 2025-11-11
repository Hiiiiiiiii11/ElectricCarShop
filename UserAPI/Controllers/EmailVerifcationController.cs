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
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
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
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var emails = await _emailVerificationService.GetAllEmailAsync();
                return Ok(emails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _emailVerificationService.DeleteEmailAsync(id);
                return Ok(new { message = "Email verification record deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
