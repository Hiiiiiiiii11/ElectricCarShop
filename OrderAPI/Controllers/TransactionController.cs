using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        public TransactionController(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }
        [HttpGet("payment/{paymentId}")]
        public async Task<IActionResult> GetByPaymentId(int paymentId)
        {
            try
            {
                var transactions = await _transactionService.GetByPaymentIdAsync(paymentId);
                return Ok(transactions);
            } catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");

            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var transaction = await _transactionService.GetByIdAsync(id);
                if (transaction == null)
                    return NotFound($"Transaction with ID {id} not found.");
                return Ok(transaction);
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
        [HttpGet("code/{code}")]
        public async Task<IActionResult> GetByTransactionCode(string code)
        {
            try
            {
                var transaction = await _transactionService.GetByTransactionCodeAsync(code);
                if (transaction == null)
                    return NotFound($"Transaction with code {code} not found.");
                return Ok(transaction);
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
        [HttpPost]
        public async Task<IActionResult> CreateTransaction([FromBody] CreateTransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var createdTransaction = await _transactionService.CreateTransactionAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = createdTransaction.Id }, createdTransaction);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, [FromBody] UpdateTransactionRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(ModelState);
                var updatedTransaction = await _transactionService.UpdateTransactionAsync(id, request);
                return Ok(updatedTransaction);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            try
            {
                var result = await _transactionService.DeleteTransactionAsync(id);
                if (!result)
                    return NotFound($"Transaction with ID {id} not found.");
                return NoContent();
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
        //[HttpGet("payment/{paymentId}/total")]
        //public async Task<IActionResult> GetTotalAmountByPayment(int paymentId)
        //{
        //    try
        //    {
        //        var totalAmount = await _transactionService.GetTotalAmountByPaymentAsync(paymentId);
        //        return Ok(new { PaymentId = paymentId, TotalAmount = totalAmount });
        //    }
        //    catch (KeyNotFoundException knfEx)
        //    {
        //        return NotFound(knfEx.Message);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}

    }
}
