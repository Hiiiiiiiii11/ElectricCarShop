using AllocationRepository.Model.DTO;
using AllocationService.Services;
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
        public async Task<IActionResult> CreateInventory([FromBody] EVInventoryRequest request)
        {
            try
            {
                var inventory = await _evInventoryService.CreateInventoryAsync(request);
                return Ok(inventory);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("increase/{vehicleId}/{quantity}")]
        public async Task<IActionResult> IncreaseInventory(int vehicleId, int quantity)
        {
            try
            {
                await _evInventoryService.IncreaseInventoryAsync(vehicleId, quantity);
                return Ok(new { message = "Tăng số lượng kho thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPost("decrease/{vehicleId}/{quantity}")]
        public async Task<IActionResult> DecreaseInventory(int vehicleId, int quantity)
        {
            try
            {
                await _evInventoryService.DecreaseInventoryAsync(vehicleId, quantity);
                return Ok(new { message = "Giảm số lượng kho thành công." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("vehicle/{id}")]
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInventories()
        {
            try
            {
                var inventories = await _evInventoryService.GetAllInventoriesAsync();
                return Ok(inventories);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        [HttpGet("total")]
        public async Task<IActionResult> GetTotalInventoryAsync()
        {
            try
            {
                var total = await _evInventoryService.GetTotalInventoryAsync();
                return Ok(new { total });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }


        }
    }
}
