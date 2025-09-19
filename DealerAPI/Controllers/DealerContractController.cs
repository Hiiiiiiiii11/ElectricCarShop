using DealerRepository.Model.DTO;
using DealerService.Services;
using Microsoft.AspNetCore.Mvc;

namespace DealerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DealerContractController : ControllerBase
    {
        private readonly IDealerContractService _dealerContractService;
        public DealerContractController(IDealerContractService dealerContractService)
        {
            _dealerContractService = dealerContractService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateDealerContract(int dealerId ,[FromBody] CreateDealerContractRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _dealerContractService.CreateDealerContractAsync(dealerId,request);
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
        [HttpGet("active/{dealerId}")]
        public async Task<IActionResult> GetActiveContractsByDealerId(int dealerId)
        {
            try
            {
                var contracts = await _dealerContractService.GetActiveByDealerIdAsync(dealerId);
                return Ok(contracts);
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

        [HttpGet("{dealerId}")]
        public async Task<IActionResult> GetContractsByDealerId(int dealerId)
        {
            try
            {
                var contracts = await _dealerContractService.GetByDealerIdAsync(dealerId);
                return Ok(contracts);
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
        [HttpGet("expired/{dealerId}")]
        public async Task<IActionResult> GetExpiredContractsByDealerId(int dealerId)
        {
            try
            {
                var contracts = await _dealerContractService.GetExpiredByDealerIdAsync(dealerId);
                return Ok(contracts);
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
        [HttpPut("renew/{contractId}")]
        public async Task<IActionResult> RenewContract(int contractId, [FromBody] RenewContractRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var contract = await _dealerContractService.RenewContractAsync(contractId, request);
                return Ok(contract);
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
        [HttpPut("terminate/{contractId}")]
        public async Task<IActionResult> TerminateContract(int contractId, [FromBody] TerminateContractRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                await _dealerContractService.TerminateContractAsync(contractId, request);
                return Ok(new { message = "Terminate Contract successful" });
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
        [HttpPut("update/{contractId}")]
        public async Task<IActionResult> UpdateContract(int contractId, [FromBody] UpdateStatusDealerContractRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _dealerContractService.UpdateStatusAsync(contractId, request);
                return Ok(new { message = "Update status successful" });
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

    }
}
