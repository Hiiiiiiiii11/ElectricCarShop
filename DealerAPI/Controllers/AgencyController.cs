using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Mvc;
using Share.ShareServices;

namespace AgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyController : Controller
    {
        private readonly IAgencyService _agencyService;
        public AgencyController(IAgencyService AgencyService)
        {
            _agencyService = AgencyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAgencys()
        {
            try
            {
                var Agencys = await _agencyService.GetAllAgencysAsync();
                return Ok(Agencys);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAgencyById(int id)
        {
            try
            {
                var Agency = await _agencyService.GetAgencyByIdAsync(id);
                return Ok(Agency);
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
        [HttpPost]
        public async Task<IActionResult> CreateAgency([FromForm] CreateAgencyRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _agencyService.CreateAgencyAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
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
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgency(int id, [FromForm] UpdateAgencyRequest request)
        {
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
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgency(int id)
        {
            try
            {
                var result = await _agencyService.DeleteAgencyAsync(id);
                return Ok(new { message = $"Delete Agency with id = {id} success" });
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
        [HttpGet("search")]
        public async Task<IActionResult> SearchAgencys([FromQuery] string term)
        {
            try
            {
                var Agencys = await _agencyService.SearchAgencysAsync(term);
                return Ok(Agencys);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
