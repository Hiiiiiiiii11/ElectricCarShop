using AgencyRepository.Model;
using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyDebtController : ControllerBase
    {
        private readonly IAgencyDebtService _AgencyDebtService;
        public AgencyDebtController(IAgencyDebtService AgencyDebtService)
        {
            _AgencyDebtService = AgencyDebtService;
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var debts = await _AgencyDebtService.GetAllDebtsAsync();
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
                var debts = await _AgencyDebtService.GetAgencysWithRemainingDebtAsync();
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("GetAll/{AgencyId}")]
        public async Task<IActionResult> GetAllByAgencyId(int AgencyId)
        {
            try
            {
                var debts = await _AgencyDebtService.GetAllDebtsByAgencyIdAsync(AgencyId);
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("Remaining/{AgencyId}")]
        public async Task<IActionResult> GetRemainingDebtsAgencyId(int AgencyId)
        {
            try
            {
                var debts = await _AgencyDebtService.GetAgencysWithRemainingDebtByAgencyIdAsync(AgencyId);
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpGet("{AgencyId}")]
        public async Task<IActionResult> GetByAgencyId(int AgencyId)
        {
            try
            {
                var debt = await _AgencyDebtService.GetDebtByAgencyIdAsync(AgencyId);
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
        [HttpPost("Add/{AgencyId}")]
        public async Task<IActionResult> AddDebt(int AgencyId, int contractId ,[FromBody] AddAgencyDebtRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _AgencyDebtService.AddDebtAsync(AgencyId, contractId, request);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while adding debt.", detail = ex.Message });
            }
        }
        [HttpPost("Payment/{AgencyId}")]
        public async Task<IActionResult> MakePayment(int AgencyId, int contractId, [FromBody] MakePaymentRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _AgencyDebtService.MakePaymentAsync(AgencyId, contractId, request);
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

        [HttpPost("Clear/{AgencyId}")]
        public async Task<IActionResult> ClearDebt(int AgencyId)
        {
            try
            {
                var result = await _AgencyDebtService.ClearDebtAsync(AgencyId);
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
                var debts = await _AgencyDebtService.SearchDebtsAsync(fromDate, toDate);
                return Ok(debts);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }





    }
}
