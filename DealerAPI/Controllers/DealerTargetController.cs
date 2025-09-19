using DealerRepository.Model.DTO;
using DealerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerTargetController : Controller
    {
        private readonly IDealerTargetService _dealerTargetService;
        public DealerTargetController(IDealerTargetService dealerTargetService)
        {
            _dealerTargetService = dealerTargetService;
        }
        [HttpPost("dealer/{dealerId}/targets")]
        public async Task<IActionResult> CreateTarget(int dealerId, [FromBody] CreateDealerTargetRequest request)
        {
            try
            {
                var result = await _dealerTargetService.CreateTargetAsync(dealerId, request);
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
                var results = await _dealerTargetService.GetAllTargetsAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("dealer/{dealerId}/current-target")]
        public async Task<IActionResult> GetCurrentTargetByDealerId(int dealerId)
        {
            try
            {
                var result = await _dealerTargetService.GetCurrentTargetByDealerIdAsync(dealerId);
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
        [HttpGet("dealer/{dealerId}/target")]
        public async Task<IActionResult> GetDealerTarget(int dealerId, [FromQuery] GetTargetReportRequest request)
        {
            try
            {
                var result = await _dealerTargetService.GetDealerTargetAsync(dealerId, request);
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
        [HttpPut("dealer/{dealerId}/target")]
        public async Task<IActionResult> UpdateDealerTarget(int dealerId,int targetId, [FromBody] UpdateDealerTargetRequest request)
        {
            try
            {
                var result = await _dealerTargetService.UpdateAchievedSalesAsync(dealerId, targetId, request);
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
        public async Task<IActionResult> GetTargetsReport([FromBody] GetTargetReportRequest request)
        {
            try
            {
                var results = await _dealerTargetService.GetTargetsReportAsync(request);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("dealer/{dealerId}")]
        public async Task<IActionResult> DeleteTarget(int dealerId ,int targetId)
        {
            try
            {
                await _dealerTargetService.RemoveDealerTarget(dealerId, targetId);
                return Ok(new {mesage = "Remove Dealer Target successfull"});
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
