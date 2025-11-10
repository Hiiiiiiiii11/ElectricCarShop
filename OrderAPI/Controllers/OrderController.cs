using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.OrderDTO;
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

        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("id")?.Value;
                if (userIdClaim != null)
                    request.CreateBy = int.Parse(userIdClaim);
                var result = await _orderService.CreateOrderAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            try
            {
                var result = await _orderService.GetAllOrdersAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("{orderId}")]
        public async Task<IActionResult> GetOrderById([FromRoute] int orderId)
        {
            try
            {
                var result = await _orderService.GetOrderByIdAsync(orderId);
                if (result == null)
                {
                    return NotFound(new { message = $"Order with ID {orderId} not found." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        //[HttpGet("customer/{customerId}")]
        //public async Task<IActionResult> GetOrdersByCustomerId([FromRoute] int customerId)
        //{
        //    try
        //    {
        //        var result = await _orderService.Get(customerId);
        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, new { message = "Internal server error: " + ex.Message });
        //    }
        //}
        [HttpPut("update/{orderId}")]
        [Authorize]
        public async Task<IActionResult> UpdateOrder([FromRoute] int orderId, [FromForm] UpdateOrderStatusRequest request)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(orderId, request);
                if (result == null)
                {
                    return NotFound(new { message = $"Order with ID {orderId} not found." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpDelete("delete/{orderId}")]
        [Authorize]
        public async Task<IActionResult> DeleteOrder([FromRoute] int orderId)
        {
            try
            {
                await _orderService.DeleteOrderAsync(orderId);
                return Ok(new { message = $"Order with ID {orderId} has been deleted." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new { message = $"Order with ID {orderId} not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("agency/{agencyId}")]

        public async Task<IActionResult> GetOrdersByAgencyId([FromRoute] int agencyId)
        {
            try
            {
                var result = await _orderService.GetOrdersByAgencyIdAsync(agencyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

    }
}
