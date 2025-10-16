using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyInventoryController : Controller
    {
       private readonly IAgencyInventoryService _AgencyInventoryService;
        public AgencyInventoryController(IAgencyInventoryService AgencyInventoryService)
        {
            _AgencyInventoryService = AgencyInventoryService;
        }
        [HttpGet("Agency/{AgencyId}/inventories")]
        public async Task<IActionResult> GetInventoriesByAgencyId(int AgencyId)
        {
            try
            {
                var inventories = await _AgencyInventoryService.GetInventoriesByAgencyIdAsync(AgencyId);
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }

        }

        [HttpGet("Agency/{AgencyId}/inventory/{vehicleId}")]
        public async Task<IActionResult> GetInventory(int AgencyId, int vehicleId)
        {
            try
            {
                var inventory = await _AgencyInventoryService.GetInventoryAsync(AgencyId, vehicleId);
                if (inventory == null)
                {
                    return NotFound($"Inventory item not found for Agency ID {AgencyId} and Variant ID {vehicleId}.");
                }
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpPost("Agency/{AgencyId}/inventory")]
        public async Task<IActionResult> CreateAgencyInventory(int AgencyId, [FromBody] CreateAgencyInventoryRequest request)
        {
            try
            {
                var inventory = await _AgencyInventoryService.CreateAgencyInventoryAsync(AgencyId, request);
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpDelete("Agency/{AgencyId}/inventory/{variantId}")]
        public async Task<IActionResult> RemoveInventoryItem(int AgencyId, int variantId)
        {
            try
            {
                await _AgencyInventoryService.RemoveInventoryItemAsync(AgencyId, variantId);
                return NoContent();
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
        [HttpPut("Agency/{id}")]                       
        public async Task<IActionResult> UpdateInventoryItem(int id, [FromBody] UpdateAgencyInventoryRequest request)
        {
            try
            {
                var updatedInventory = await _AgencyInventoryService.UpdateInventoryAsync(id, request);
                return Ok(updatedInventory);
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
    }
}
