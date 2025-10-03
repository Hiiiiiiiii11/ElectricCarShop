
using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AgencyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgencyContractController : ControllerBase
    {
        private readonly IAgencyContractService _AgencyContractService;
        public AgencyContractController(IAgencyContractService AgencyContractService)
        {
            _AgencyContractService = AgencyContractService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateAgencyContract(int AgencyId ,[FromBody] CreateAgencyContractRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var result = await _AgencyContractService.CreateAgencyContractAsync(AgencyId,request);
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
        [HttpGet("active/{AgencyId}")]
        public async Task<IActionResult> GetActiveContractsByAgencyId(int AgencyId)
        {
            try
            {
                var contracts = await _AgencyContractService.GetActiveByAgencyIdAsync(AgencyId);
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

        [HttpGet("{AgencyId}")]
        public async Task<IActionResult> GetContractsByAgencyId(int AgencyId)
        {
            try
            {
                var contracts = await _AgencyContractService.GetByAgencyIdAsync(AgencyId);
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
        [HttpGet("expired/{AgencyId}")]
        public async Task<IActionResult> GetExpiredContractsByAgencyId(int AgencyId)
        {
            try
            {
                var contracts = await _AgencyContractService.GetExpiredByAgencyIdAsync(AgencyId);
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
                var contract = await _AgencyContractService.RenewContractAsync(contractId, request);
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
                await _AgencyContractService.TerminateContractAsync(contractId, request);
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
        public async Task<IActionResult> UpdateContract(int contractId, [FromBody] UpdateStatusAgencyContractRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _AgencyContractService.UpdateStatusAsync(contractId, request);
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
