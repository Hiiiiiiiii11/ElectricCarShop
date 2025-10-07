using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AllocationController : Controller
    {
        private readonly IAllocationService _allocationService;
        public AllocationController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAllocation([FromBody] AllocationRequestModel request)
        {
            try
            {
                var allocation = await _allocationService.CreateAsync(request);
                return Ok(allocation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("agency/{agencyId}")]
        public async Task<IActionResult> GetByAgencyId(int agencyId)
        {
            try
            {
                var allocations = await _allocationService.GetByAgencyIdAsync(agencyId);
                return Ok(allocations);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("agency/{agencyId}/vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByAgencyAndVehicle(int agencyId, int vehicleId)
        {
            try
            {
                var allocation = await _allocationService.GetByAgencyAndVehicleAsync(agencyId, vehicleId);
                if (allocation == null)
                    return NotFound(new { message = $"Không tìm thấy phân bổ cho đại lý {agencyId} và xe {vehicleId}" });
                return Ok(allocation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetByVehicleId(int vehicleId)
        {
            try
            {
                var allocations = await _allocationService.GetByVehicleIdAsync(vehicleId);
                return Ok(allocations);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("inventory/{evInventoryId}")]
        public async Task<IActionResult> GetByInventoryId(int evInventoryId)
        {
            try
            {
                var allocation = await _allocationService.GetByInventoryIdAsync(evInventoryId);
                if (allocation == null)
                    return NotFound(new { message = $"Không tìm thấy phân bổ cho inventory ID {evInventoryId}" });
                return Ok(allocation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }



    }
}
    
