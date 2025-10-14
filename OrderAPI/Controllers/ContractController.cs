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
        [HttpGet("GetContractById/{id}")]
        public IActionResult GetContractById(int id)
        {
            var contract =  _contractService.GetContractByIdAsync(id);
            if (contract == null)
            {
                return NotFound();
            }
            return Ok(contract);
        }
        [HttpGet("GetContractsByQuotationId/{quotationId}")]
        public IActionResult GetContractsByQuotationId(int quotationId)
        {
            var contracts = _contractService.GetContractsByQuotationIdAsync(quotationId);
            return Ok(contracts);
        }
        [HttpGet("GetByContractNumber/{contractNumber}")]
        public IActionResult GetByContractNumber(string contractNumber)
        {
            var contract = _contractService.GetByContractNumberAsync(contractNumber);
            if (contract == null)
            {
                return NotFound();
            }
            return Ok(contract);
        }
        [HttpPost("CreateContract")]
        public IActionResult CreateContract([FromBody] CreateContractRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }
            try
            {
                var createdContract = _contractService.CreateContractAsync(request);
                return Ok(createdContract);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("UpdateContract/{id}")]
        public IActionResult UpdateContract(int id, [FromBody] UpdateContractRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }
            try
            {
                var updatedContract = _contractService.UpdateContractAsync(id, request);
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
        public IActionResult DeleteContract(int id)
        {
            try
            {
                var result =  _contractService.DeleteContractAsync(id);
                if (result == null)
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
