using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize]
        public async Task<IActionResult> CreateAllocation([FromForm] AllocationRequestModel request)
        {
            try
            {
                var allocation = await _allocationService.CreateAsync(request);
                return Ok(allocation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [Authorize]
        [HttpGet("agency/{agencyContractId}")]
        public async Task<IActionResult> GetByAgencyId(int agencyContractId)
        {
            try
            {
                var allocations = await _allocationService.GetByAgencyContractIdAsync(agencyContractId);
                return Ok(allocations);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [Authorize]
        [HttpGet("agency/{agencyContractId}/vehicle/{vehicleInstanceId}")]
        public async Task<IActionResult> GetByAgencyAndVehicle(int agencyContractId, int vehicleInstanceId)
        {
            try
            {
                var allocation = await _allocationService.GetByAgencyContractAndVehicleAsync(agencyContractId, vehicleInstanceId);
                if (allocation == null)
                    return NotFound(new { message = $"Không tìm thấy phân bổ cho đại lý {agencyContractId} và xe {vehicleInstanceId}" });
                return Ok(allocation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [Authorize]
        [HttpGet("vehicle/{vehicleInstanceId}")]
        public async Task<IActionResult> GetByVehicleInstanceId(int vehicleInstanceId)
        {
            try
            {
                var allocations = await _allocationService.GetByVehicleInstanceIdAsync(vehicleInstanceId);
                return Ok(allocations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [Authorize]
        [HttpGet("agencyOrder/{agencyOrderId}")]
        public async Task<IActionResult> GetByAgencyOrderId(int agencyOrderId)
        {
            try
            {
                var allocations = await _allocationService.GetByAgencyOrderIdAsync(agencyOrderId);
                return Ok(allocations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("GetAllAllocations")]
        public async Task<IActionResult> GetAllAllocations()
        {
            try
            {
                var allocations = await _allocationService.GetAllocationsAsync();
                return Ok(allocations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAllocation(int id, [FromForm] AllocationUpdateModel request)
        {
            try
            {
                var allocation = await _allocationService.UpdateAsync(id, request);
                return Ok(allocation);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAllocation(int id)
        {
            try
            {
                await _allocationService.DeleteAsync(id);
                return Ok(new { message = "Xóa phân bổ thành công." });
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }

}
    
