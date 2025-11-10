using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.OrderDTO;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InstallmentPaymentController : ControllerBase
    {
        private readonly IInstallmentPaymentsService _installmentPaymentService;

        public InstallmentPaymentController(IInstallmentPaymentsService installmentPaymentService)
        {
            _installmentPaymentService = installmentPaymentService;
        }

        [HttpPost("process-installment")]
        [Authorize]
        public async Task<IActionResult> ProcessInstallmentPayment([FromBody] InstallmentPaymentRequest request)
        {
            try
            {
                var result = await _installmentPaymentService.CreateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("by-plan/{planId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentsByPlanId([FromRoute] int planId)
        {
            try
            {
                var result = await _installmentPaymentService.GetPaymentsByPlanIdAsync(planId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("by-item/{itemId}")]
        [Authorize]
        public async Task<IActionResult> GetPaymentsByItemId([FromRoute] int itemId)
        {
            try
            {
                var result = await _installmentPaymentService.GetPaymentsByItemIdAsync(itemId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpPut("{paymentId}")]
        [Authorize]
        public async Task<IActionResult> UpdateInstallmentPayment([FromRoute] int paymentId, [FromForm] UpdateInstallmentPaymentRequest request)
        {
            try
            {
                var result = await _installmentPaymentService.UpdatePaymentAsync(paymentId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteInstallmentPayment([FromRoute] int id)
        {
            try
            {
                await _installmentPaymentService.DeleteAsync(id);
                return Ok(new { message = "Delete payment success" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
