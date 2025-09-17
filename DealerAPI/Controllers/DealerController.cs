using DealerRepository.Model.DTO;
using DealerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerController : Controller
    {
        private readonly IDealerService _dealerService;
        public DealerController(IDealerService dealerService)
        {
            _dealerService = dealerService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDealers()
        {
            try
            {
                var dealers = await _dealerService.GetAllDealersAsync();
                return Ok(dealers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDealerById(int id)
        {
            try
            {
                var dealer = await _dealerService.GetDealerByIdAsync(id);
                return Ok(dealer);
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
        public async Task<IActionResult> CreateDealer([FromForm] CreateDealerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _dealerService.CreateDealerAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDealer(int id, [FromForm] UpdateDealerRequest request)
        {
            try
            {
                var result = await _dealerService.UpdateDealerAsync(id, request);
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
        public async Task<IActionResult> DeleteDealer(int id)
        {
            try
            {
                var result = await _dealerService.DeleteDealerAsync(id);
                return Ok(new { message = $"Delete dealer with id = {id} success" });
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
        public async Task<IActionResult> SearchDealers([FromQuery] string term)
        {
            try
            {
                var dealers = await _dealerService.SearchDealersAsync(term);
                return Ok(dealers);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
