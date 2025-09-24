using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleController : Controller
    {
        private readonly IVehicleService _vehicleService;
        public VehicleController(IVehicleService vehicleService)
        {
            _vehicleService = vehicleService;
        }

        // Controller methods would go here
        [HttpGet]
        public async Task<IActionResult> GetAllVehicles()
        {
            try { 
            var vehicles = await _vehicleService.GetAllVehiclesAsync();
            return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleById(int id)
        {
            try
            {
                var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
                return Ok(vehicle);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPost]
        public async Task<IActionResult> AddVehicle([FromBody] CreateVehicleRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _vehicleService.AddVehicleAsync(request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(int id, [FromBody] UpdateVehicleRequest request)
        {
            try
            {
                await _vehicleService.UpdateVehicleAsync(id, request);
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(int id)
        {
            try
            {
                await _vehicleService.DeleteVehicleAsync(id);
                return Ok(new {message = "Delete vehicle successful"});
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("search")]
        public async Task<IActionResult> SearchVehicles([FromQuery] string? variantName, [FromQuery] string? color, [FromQuery] string? batteryCapacity)
        {
            try
            {
                var vehicles = await _vehicleService.SearchVehiclesAsync(variantName, color, batteryCapacity);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetVehiclesByStatus(string status)
        {
            try
            {
                var vehicles = await _vehicleService.GetVehiclesByStatusAsync(status);
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("available-stock")]
        public async Task<IActionResult> GetVehiclesWithAvailableStock()
        {
            try
            {
                var vehicles = await _vehicleService.GetVehiclesWithAvailableStockAsync();
                return Ok(vehicles);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{vehicleId}/current-price")]
        public async Task<IActionResult> GetCurrentPrice(int vehicleId, [FromQuery] DateTime date)
        {
            try
            {
                var price = await _vehicleService.GetCurrentPriceAsync(vehicleId, date);
                return Ok(price);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    }
}
