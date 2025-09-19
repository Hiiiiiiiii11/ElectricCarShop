using DealerRepository.Model.DTO;
using DealerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerDebtController : ControllerBase
    {
        private readonly IDealerDebtService _dealerDebtService;
        public DealerDebtController(IDealerDebtService dealerDebtService)
        {
            _dealerDebtService = dealerDebtService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var debts = await _dealerDebtService.GetAllDebtsAsync();
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("Remaining")]
        public async Task<IActionResult> GetRemainingDebts()
        {
            try
            {
                var debts = await _dealerDebtService.GetDealersWithRemainingDebtAsync();
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{dealerId}")]
        public async Task<IActionResult> GetByDealerId(int dealerId)
        {
            try
            {
                var debt = await _dealerDebtService.GetDebtByDealerIdAsync(dealerId);
                return Ok(debt);
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
        [HttpPost("Add/{dealerId}")]
        public async Task<IActionResult> AddDebt(int dealerId, [FromBody] AddDealerDebtRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _dealerDebtService.AddDebtAsync(dealerId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", detail = ex.Message });
            }
        }
        [HttpPost("Payment/{dealerId}")]
        public async Task<IActionResult> MakePayment(int dealerId, [FromBody] MakePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _dealerDebtService.MakePaymentAsync(dealerId, request);
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
                return StatusCode(500, new { message = "An error occurred while processing your request.", detail = ex.Message });
            }
        }

        [HttpPost("Clear/{dealerId}")]
        public async Task<IActionResult> ClearDebt(int dealerId)
        {
            try
            {
                var result = await _dealerDebtService.ClearDebtAsync(dealerId);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", detail = ex.Message });
            }
        }

        [HttpGet("Search")]
        public async Task<IActionResult> SearchDebts([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            try
            {
                var debts = await _dealerDebtService.SearchDebtsAsync(fromDate, toDate);
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }





    }
}
