using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstallmentItemController : Controller
    {
        private readonly IInstallmentItemsService _installmentItemService;
        public InstallmentItemController(IInstallmentItemsService installmentItemService)
        {
            _installmentItemService = installmentItemService;
        }
        [HttpGet("plan/{planId}")]
        public async Task<IActionResult> GetByPlanIdAsync(int planId)
        {
            try
            {
                var result = await _installmentItemService.GetByPlanIdAsync(planId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }

        }

        [HttpPost]
        public async Task<IActionResult> CreateInstallmentItem([FromForm] InstallmentItemRequest request)
        {
            try
            {
                var result = await _installmentItemService.CreateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            try
            {
                var result = await _installmentItemService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            try
            {
                var result = await _installmentItemService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            try
            {
                await _installmentItemService.DeleteAsync(id);
                return Ok(new { message = $"Installment item with ID {id} has been deleted." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
