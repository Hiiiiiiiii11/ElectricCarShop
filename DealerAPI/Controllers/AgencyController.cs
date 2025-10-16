using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Greet;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : ControllerBase
    {
        private readonly IAgencyService _agencyService;

        public AgencyController(IAgencyService agencyService)
        {
            _agencyService = agencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAgencys()
        {
            try
            {
                var agencys = await _agencyService.GetAllAgencysAsync();
                return Ok(agencys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgencyById(int id)
        {
            try
            {
                var agency = await _agencyService.GetAgencyByIdAsync(id);
                return Ok(agency);
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
        [Authorize(Roles = "EVManager")]
        [HttpPost]
        public async Task<IActionResult> CreateAgency([FromForm] CreateAgencyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _agencyService.CreateAgencyAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [Authorize(Roles = "AgencyManager")]
        [HttpPost("{agencyId}/assign-user")]
        public async Task<IActionResult> AssignUserToAgency(int agencyId, [FromBody] AssignUserAgencyRequest request)
        {
            try
            {
                var result = await _agencyService.AssignUserAsync(request, agencyId);
                if (!result)
                    return BadRequest(new { message = "Failed to assign user to agency" });

                return Ok(new { message = "User assigned successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "AgencyManager")]
        [HttpPost("{agencyId}/remove-user")]
        public async Task<IActionResult> RemoveUserFromAgency(int agencyId, [FromBody] RemoveUserAgencyRequest request)
        {
            try
            {
                var result = await _agencyService.RemoveUserAsync(request, agencyId);
                if (!result)
                    return BadRequest(new { message = "Failed to remove user from agency" });

                return Ok(new { message = "User removed successfully" });
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
        [Authorize(Roles = "EVManager,AgencyManager")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgency(int id, [FromForm] UpdateAgencyRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _agencyService.UpdateAgencyAsync(id, request);
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
        [Authorize(Roles = "EVManager")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgency(int id)
        {
            try
            {
                await _agencyService.DeleteAgencyAsync(id);
                return Ok(new { message = $"Delete Agency with id = {id} success" });
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

        [HttpGet("search")]
        public async Task<IActionResult> SearchAgencys([FromQuery] string term)
        {
            try
            {
                var agencys = await _agencyService.SearchAgencysAsync(term);
                return Ok(agencys);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
