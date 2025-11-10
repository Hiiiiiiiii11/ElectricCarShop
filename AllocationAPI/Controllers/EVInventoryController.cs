using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EVInventoryController : Controller
    {
        private readonly IEVInventoryService _evInventoryService;
        public EVInventoryController(IEVInventoryService evInventoryService)
        {
            _evInventoryService = evInventoryService;
        }
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateInventory([FromBody] EVInventoryRequest request)
        {
            try
            {
                var inventory = await _evInventoryService.CreateInventoryAsync(request);
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteInventory(int id)
        {
            try
            {
                await _evInventoryService.DeleteInventoryAsync(id);
                return Ok(new { message = "Xóa kho thành công." });
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

        [HttpGet("vehicle/{id}")]
        [Authorize]
        public async Task<IActionResult> GetInventoryById(int id)
        {
            try
            {
                var inventory = await _evInventoryService.GetByVehicleIdAsync(id);
                if (inventory == null)
                    return NotFound(new { message = $"Không tìm thấy kho với ID {id}" });
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAllInventories()
        {
            try
            {
                var inventories = await _evInventoryService.GetAllInventoriesAsync();
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
