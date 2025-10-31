using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehicleInstanceController : Controller
    {
        private readonly IVehicleInstanceService _vehicleInstanceService;
        public VehicleInstanceController(IVehicleInstanceService vehicleInstanceService)
        {
            _vehicleInstanceService = vehicleInstanceService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] CreateVehicleInstanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _vehicleInstanceService.CreateAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateVehicleInstanceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var result = await _vehicleInstanceService.UpdateAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("get/{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _vehicleInstanceService.GetByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _vehicleInstanceService.GetAllAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _vehicleInstanceService.DeleteAsync(id);
                return Ok(new { message = "Vehicle instance deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicleId(int vehicleId)
        {
            try
            {
                var result = await _vehicleInstanceService.GetByVehicleIdAsync(vehicleId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

    }
}
