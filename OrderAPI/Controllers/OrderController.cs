using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var orders = await _orderService.GetAllOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetOrderById(int id)
        {
            try
            {
                var order = await _orderService.GetOrderByIdAsync(id);
                return Ok(order);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is null." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var createdOrder = await _orderService.CreateOrderAsync(request);
                // Có thể dùng CreatedAtAction nếu bạn muốn trả Location header:
                // return CreatedAtAction(nameof(GetOrderById), new { id = createdOrder.Id }, createdOrder);
                return Ok(createdOrder);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateOrder(int id, [FromBody] UpdateOrderRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is null." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var updatedOrder = await _orderService.UpdateOrderAsync(id, request);
                return Ok(updatedOrder);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (InvalidOperationException invEx)
            {
                return BadRequest(new { message = invEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);
                if (!result)
                    return NotFound(new { message = $"Order with ID {id} not found." });

                return Ok(new { message = $"Order with ID {id} deleted successfully." });
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("customer/{customerId:int}")]
        public async Task<IActionResult> GetOrdersByCustomerId(int customerId)
        {
            try
            {
                var orders = await _orderService.GetOrdersByCustomerIdAsync(customerId);
                if (orders == null || !orders.Any())
                    return NotFound(new { message = $"No orders found for customer ID {customerId}." });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetOrdersByStatus(string status)
        {
            try
            {
                var orders = await _orderService.GetOrdersByStatusAsync(status);
                if (orders == null || !orders.Any())
                    return NotFound(new { message = $"No orders found with status '{status}'." });

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
