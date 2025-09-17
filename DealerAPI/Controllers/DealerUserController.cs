using DealerRepository.Model.DTO;
using DealerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerUserController : Controller
    {
        private readonly IDealerUserService _dealerUserService;
        public DealerUserController(IDealerUserService dealerUserService)
        {
            _dealerUserService = dealerUserService;
        }

        [HttpGet("GetAll/{dealerId}")]
        public async Task<IActionResult> GetAllDealerUsersByDealerId(int dealerId)
        {
            try
            {
                var dealerUsers = await _dealerUserService.GetDealerUsersByDealerIdAsync(dealerId);
                return Ok(dealerUsers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("DealerUser/{userId}")]
        public async Task<IActionResult> GetDealerUserById(int userId)
        {
            try
            {
                var dealerUser = await _dealerUserService.GetDealerUserByIdAsync(userId);
                return Ok(dealerUser);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost]
        public async Task<IActionResult> CreateDealerUser([FromForm] CreateDealerUserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _dealerUserService.CreateDealerUserAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete]
        public async Task<IActionResult> RemoveDealerUser([FromQuery] int userId, [FromQuery] int dealerId)
        {
            try
            {
                await _dealerUserService.RemoveDealerUserAsync(userId, dealerId);
                return Ok(new { message = "DealerUser removed successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
