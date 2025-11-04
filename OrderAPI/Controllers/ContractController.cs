using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContractController : Controller
    {
        private readonly IContractService _contractService;
        private readonly IContractEmailService _contractEmailService;

        public ContractController(IContractService contractService, IContractEmailService contractEmailService)
        {
            _contractService = contractService;
            _contractEmailService = contractEmailService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractByIdAsync(int id)
        {
            try
            {
                var contract = await _contractService.GetContractByIdAsync(id);
                if (contract == null)
                    return NotFound("Contract not found");
                return Ok(contract);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("quotation/{quotationId}")]
        public async Task<IActionResult> GetContractsByQuotationId(int quotationId)
        {
            try
            {
                var contracts = await _contractService.GetContractsByQuotationIdAsync(quotationId);
                return Ok(contracts);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("number/{contractNumber}")]
        public async Task<IActionResult> GetByContractNumber(string contractNumber)
        {
            try
            {
                var contract = await _contractService.GetByContractNumberAsync(contractNumber);
                if (contract == null)
                    return NotFound("Contract not found");
                return Ok(contract);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("getall/{agencyId}")]
        public async Task<IActionResult> GetAllContractByAgencyId(int agencyId)
        {
            try
            {
                var contract = await _contractService.GetAllContractByAgencyId(agencyId);
                if (contract == null)
                    return NotFound("Contract not found");
                return Ok(contract);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request)
        {
            if (request == null)
                return BadRequest("Request body is null.");

            try
            {
                var createdContract = await _contractService.CreateContractAsync(request);
                return Ok(createdContract);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromForm] UpdateContractRequest request)
        {
            if (request == null)
                return BadRequest("Request body is null.");

            try
            {
                var updatedContract = await _contractService.UpdateContractAsync(id, request);
                return Ok(updatedContract);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Contract not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            try
            {
                var result = await _contractService.DeleteContractAsync(id);
                if (!result)
                    return NotFound("Contract not found");

                return Ok("Contract deleted successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpPost("sent-contracr-email/{contractId}")]
        public async Task<IActionResult> SendContractEmail(int contractId, string customerEmail,IFormFile file)
        {
            try
            {
                await _contractEmailService.UploadAndUpdateContractEmailAsync(contractId, customerEmail, file);
                return Ok(new { message = "Contract email sent successfully." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Contract not found");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
