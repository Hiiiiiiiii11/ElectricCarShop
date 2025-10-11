using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehicleOptionController : Controller
    {
        private readonly IVehicleOptionService _vehicleOptionService;
        public VehicleOptionController(IVehicleOptionService vehicleOptionService)
        {
            _vehicleOptionService = vehicleOptionService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateVehicleOption([FromBody] VehicleOptionRequest request)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _vehicleOptionService.CreateAsync(request);
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
        public async Task<IActionResult> UpdateVehicleOption(int id, [FromBody] UpdateVehicleOptionRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _vehicleOptionService.UpdateAsync(id, request);
                if (result == null)
                    return NotFound(new { message = $"Vehicle option {id} not found" });
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

        [HttpGet]
        public async Task<IActionResult> GetAllVehicleOptions()
        {
            try
            {
                var result = await _vehicleOptionService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicleOptionById(int id)
        {
            try
            {
                var result = await _vehicleOptionService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = $"Vehicle option {id} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("model/{modelName}")]
        public async Task<IActionResult> GetVehicleOptionByModelName(string modelName)
        {
            try
            {
                var result = await _vehicleOptionService.GetByModelNameAsync(modelName);
                if (result == null)
                    return NotFound(new { message = $"Vehicle option with model name {modelName} not found" });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicleOption(int id)
        {
            try
            {
                await _vehicleOptionService.DeleteAsync(id);
                return NoContent();
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

    }
}
