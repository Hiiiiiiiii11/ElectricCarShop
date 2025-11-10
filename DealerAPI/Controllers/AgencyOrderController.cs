using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyOrderController : Controller
    {
        private readonly IAgencyOrderService _agencyOrderService;
        public AgencyOrderController(IAgencyOrderService agencyOrderService)
        {
            _agencyOrderService = agencyOrderService;
        }
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateAgencyOrder([FromBody] CreateAgencyOrderRequest request)
        {
            try
            {
                var result = await _agencyOrderService.CreateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpPut("update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateAgencyOrder([FromRoute] int id,[FromForm] UpdateAgencyOrderRequest request)
        {
            try
            {
                var result = await _agencyOrderService.UpdateAsync(id,request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("get-by-agency/{agencyId}")]
        public async Task<IActionResult> GetOrdersByAgencyId([FromRoute] int agencyId)
        {
            try
            {
                var result = await _agencyOrderService.GetByAgencyAsync(agencyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("get-by-contract/{contractId}")]
        public async Task<IActionResult> GetOrdersByContractId([FromRoute] int contractId)
        {
            try
            {
                var result = await _agencyOrderService.GetByContractAsync(contractId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpDelete("delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteAgencyOrder([FromRoute] int id)
        {
            try
            {
                var success = await _agencyOrderService.DeleteAsync(id);
                if (!success)
                    return NotFound(new { message = $"Agency order with ID {id} not found." });
                else
                return Ok(new { message = "Agency order deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetAgencyOrderById([FromRoute] int id)
        {
            try
            {
                var result = await _agencyOrderService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }

        }
        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllAgencyOrders()
        {
            try
            {
                var result = await _agencyOrderService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
