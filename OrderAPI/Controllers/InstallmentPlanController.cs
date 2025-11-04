using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InstallmentPlanController : Controller
    {
        private readonly IInstallmentPlansService _installmentPlansService;
        public InstallmentPlanController(IInstallmentPlansService installmentPlansService)
        {
            _installmentPlansService = installmentPlansService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateInstallmentPlan([FromBody] InstallmentPlanRequest request)
        {
            try
            {
                var result = await _installmentPlansService.CreateInstallmentPlanAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstallmentPlanById([FromRoute] int id)
        {
            try
            {
                var result = await _installmentPlansService.GetByIdAsync(id);
                if (result == null)
                {
                    return NotFound(new { message = $"Installment Plan with ID {id} not found." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }

        }
        [HttpGet]
        public async Task<IActionResult> GetAllInstallmentPlans()
        {
            try
            {
                var result = await _installmentPlansService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInstallmentPlan([FromRoute] int id, [FromForm] InstallmentPlanUpdateRequest request)
        {
            try
            {
                var result = await _installmentPlansService.UpdateInstallmentPlanAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstallmentPlan([FromRoute] int id)
        {
            try
            {
                await _installmentPlansService.DeleteAsync(id);
                return Ok(new { message = "Delete plan success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}