using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTransactions()
        {
            try
            {
                var transactions = await _transactionService.GetAllTransactionAsync();
                return Ok(transactions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> GetByPaymentId(int paymentId)
        {
            try
            {
                var transactions = await _transactionService.GetByPaymentIdAsync(paymentId);
                if (transactions == null || !transactions.Any())
                    return NotFound(new { message = "Not found" });

                return Ok(transactions);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetByIdAsync(id);
                if (transaction == null)
                    return NotFound(new { message = "Not found" });

                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByTransactionCode(string code)
        {
            try
            {
                var transaction = await _transactionService.GetByTransactionCodeAsync(code);
                if (transaction == null)
                    return NotFound(new { message = "Not found" });

                return Ok(transaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data." });

                var createdTransaction = await _transactionService.CreateTransactionAsync(request);
                return Ok(createdTransaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data." });

                var updatedTransaction = await _transactionService.UpdateTransactionAsync(id, request);
                return Ok(updatedTransaction);
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var result = await _transactionService.DeleteTransactionAsync(id);
                if (!result)
                    return NotFound(new { message = "Not found" });

                return Ok(new { message = "Deleted successfully" });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = "Not found" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
