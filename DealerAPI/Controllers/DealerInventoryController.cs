using DealerRepository.Model.DTO;
using DealerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerInventoryController : Controller
    {
       private readonly IDealerInventoryService _dealerInventoryService;
        public DealerInventoryController(IDealerInventoryService dealerInventoryService)
        {
            _dealerInventoryService = dealerInventoryService;
        }
        [HttpGet("dealer/{dealerId}/inventories")]
        public async Task<IActionResult> GetInventoriesByDealerId(int dealerId)
        {
            try
            {
                var inventories = await _dealerInventoryService.GetInventoriesByDealerIdAsync(dealerId);
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }

        [HttpGet("dealer/{dealerId}/inventory/{vehicleId}")]
        public async Task<IActionResult> GetInventory(int dealerId, int vehicleId)
        {
            try
            {
                var inventory = await _dealerInventoryService.GetInventoryAsync(dealerId, vehicleId);
                if (inventory == null)
                {
                    return NotFound($"Inventory item not found for Dealer ID {dealerId} and Variant ID {vehicleId}.");
                }
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpPost("dealer/{dealerId}/inventory")]
        public async Task<IActionResult> CreateDealerInventory(int dealerId, [FromBody] CreateDealerInventoryRequest request)
        {
            try
            {
                var inventory = await _dealerInventoryService.CreateDealerInventoryAsync(dealerId, request);
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }

        [HttpDelete("dealer/{dealerId}/inventory/{variantId}")]
        public async Task<IActionResult> RemoveInventoryItem(int dealerId, int variantId)
        {
            try
            {
                await _dealerInventoryService.RemoveInventoryItemAsync(dealerId, variantId);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", detail = ex.Message });
            }
        }
        [HttpPut("dealer/{dealerId}/inventory/{variantId}/quantity")]
        public async Task<IActionResult> UpdateInventoryQuantity(int dealerId, int vehicleId, [FromBody] int newQuantity)
        {
            try
            {
                await _dealerInventoryService.SetQuantityAsync(dealerId, vehicleId, newQuantity);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", detail = ex.Message });
            }
        }

        [HttpGet("dealer/{dealerId}/inventory/{variantId}/sufficient-stock")]
        public async Task<IActionResult> HasSufficientStock(int dealerId, int vehicleId, [FromQuery] int requiredQuantity)
        {
            try
            {
                var hasStock = await _dealerInventoryService.HasSufficientStockAsync(dealerId, vehicleId, requiredQuantity);
                return Ok(new { hasSufficientStock = hasStock });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        [HttpPut("dealer/{dealerId}/inventory/{variantId}/adjust-quantity")]
        public async Task<IActionResult> AdjustInventoryQuantity(int dealerId, int vehicleId, [FromBody] int quantityChange)
        {
            try
            {
                await _dealerInventoryService.SetQuantityAsync(dealerId, vehicleId, quantityChange);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", detail = ex.Message });
            }
        }
    }
}
