using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }
        [HttpGet("GetTotalPaidByOrder/{orderId}")]
        public IActionResult GetTotalPaidByOrder(int orderId)
        {
            var totalPaid = _paymentService.GetTotalPaidByOrderAsync(orderId);
            return Ok(totalPaid);
        }
        // Additional endpoints for payment operations can be added here    
        [HttpGet("GetPaymentById/{id}")]
        public IActionResult GetPaymentById(int id)
        {
            var payment = _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound($"Payment with ID {id} not found.");
            }
            return Ok(payment);
        }
        [HttpGet("GetPaymentsByOrderId/{orderId}")]
        public IActionResult GetPaymentsByOrderId(int orderId)
        {
            var payments = _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }
        [HttpGet("GetPaymentsByStatus/{status}")]
        public IActionResult GetPaymentsByStatus(string status)
        {
            var payments = _paymentService.GetPaymentsByStatusAsync(status);
            return Ok(payments);
        }
        [HttpPost("CreatePayment")]
        public IActionResult CreatePayment([FromBody] CreatePaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }
            try
            {
                var createdPayment = _paymentService.CreatePaymentAsync(request);
                return Ok(createdPayment);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("UpdatePayment/{id}")]
        public IActionResult UpdatePayment(int id, [FromBody] UpdatePaymentRequest request)
        {
            if (request == null)
            {
                return BadRequest("Request body is null.");
            }
            try
            {
                var updatedPayment = _paymentService.UpdatePaymentAsync(id, request);
                return Ok(updatedPayment);
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
        [HttpDelete("DeletePayment/{id}")]
        public IActionResult DeletePayment(int id)
        {
            try
            {
                var result = _paymentService.DeletePaymentAsync(id);
                if (result == null)
                {
                    return NotFound($"Payment with ID {id} not found.");
                }
                return Ok($"Payment with ID {id} deleted successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }

        }

    }
}
