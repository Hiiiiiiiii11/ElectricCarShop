using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyTargetController : Controller
    {
        private readonly IAgencyTargetService _AgencyTargetService;
        public AgencyTargetController(IAgencyTargetService AgencyTargetService)
        {
            _AgencyTargetService = AgencyTargetService;
        }
        [HttpPost("Agency/{AgencyId}/targets")]
        public async Task<IActionResult> CreateTarget(int AgencyId, [FromBody] CreateAgencyTargetRequest request)
        {
            try
            {
                var result = await _AgencyTargetService.CreateTargetAsync(AgencyId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("targets")]
        public async Task<IActionResult> GetAllTargets([FromQuery] GetTargetReportRequest request)
        {
            try
            {
                var results = await _AgencyTargetService.GetAllTargetsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("Agency/{AgencyId}/current-target")]
        public async Task<IActionResult> GetCurrentTargetByAgencyId(int AgencyId)
        {
            try
            {
                var result = await _AgencyTargetService.GetCurrentTargetByAgencyIdAsync(AgencyId);
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
        [HttpGet("Agency/{AgencyId}/target")]
        public async Task<IActionResult> GetAgencyTarget(int AgencyId, [FromQuery] GetTargetReportRequest request)
        {
            try
            {
                var result = await _AgencyTargetService.GetAgencyTargetAsync(AgencyId, request);
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
        [HttpPut("Agency/{AgencyId}/target")]
        public async Task<IActionResult> UpdateAgencyTarget(int AgencyId,int targetId, [FromBody] UpdateAgencyTargetRequest request)
        {
            try
            {
                var result = await _AgencyTargetService.UpdateAchievedSalesAsync(AgencyId, targetId, request);
                return Ok(result);
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
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetTargetsReport([FromForm] GetTargetReportRequest request)
        {
            try
            {
                var results = await _AgencyTargetService.GetTargetsReportAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("Agency/{AgencyId}")]
        public async Task<IActionResult> DeleteTarget(int AgencyId ,int targetId)
        {
            try
            {
                await _AgencyTargetService.RemoveAgencyTarget(AgencyId, targetId);
                return Ok(new {mesage = "Remove Agency Target successfull"});
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
    }
}
