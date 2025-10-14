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

        public ContractController(IContractService contractService)
        {
            _contractService = contractService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetContractByIdAsync(int id)
        {
            var contract = await _contractService.GetContractByIdAsync(id);
            if (contract == null)
                return NotFound(new { message = $"Contract with id {id} not found." });

            return Ok(contract);
        }

        [HttpGet("GetContractsByQuotationId/{quotationId}")]
        public async Task<IActionResult> GetContractsByQuotationId(int quotationId)
        {
            var contracts = await _contractService.GetContractsByQuotationIdAsync(quotationId);
            return Ok(contracts);
        }

        [HttpGet("GetByContractNumber/{contractNumber}")]
        public async Task<IActionResult> GetByContractNumber(string contractNumber)
        {
            var contract = await _contractService.GetByContractNumberAsync(contractNumber);
            if (contract == null)
            {
                return NotFound(new {message= $"Contract with number {contractNumber} not found." });
            }
            return Ok(contract);
        }

        [HttpPost("CreateContract")]
        public async Task<IActionResult> CreateContract([FromBody] CreateContractRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var createdContract = await _contractService.CreateContractAsync(request);
                return Ok(createdContract);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("UpdateContract/{id}")]
        public async Task<IActionResult> UpdateContract(int id, [FromBody] UpdateContractRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }

            try
            {
                var updatedContract = await _contractService.UpdateContractAsync(id, request);
                return Ok(updatedContract);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("DeleteContract/{id}")]
        public async Task<IActionResult> DeleteContract(int id)
        {
            try
            {
                var result = await _contractService.DeleteContractAsync(id);
                if (!result)
                {
                    return NotFound($"Contract with ID {id} not found.");
                }
                return Ok($"Contract with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
