using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services; // nơi khai báo IOrderDetailService
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderDetailController : ControllerBase
    {
        private readonly IOrderDetailService _orderDetailService;

        public OrderDetailController(IOrderDetailService orderDetailService)
        {
            _orderDetailService = orderDetailService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateOrderDetail([FromBody] CreateOrderDetailRequest request)
        {
            try
            {
                var result = await _orderDetailService.CreateOrderDetailAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrderDetailById([FromRoute] int id)
        {
            try
            {
                var result = await _orderDetailService.GetOrderDetailByIdAsync(id);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpGet("get-by-order/{orderId}")]
        public async Task<IActionResult> GetOrderDetailsByOrderId([FromRoute] int orderId)
        {
            try
            {
                var result = await _orderDetailService.GetOrderDetailsByOrderIdAsync(orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateOrderDetailPrice([FromRoute] int id, [FromBody] UpdateOrderDetailPriceRequest request)
        {
            try
            {
                var result = await _orderDetailService.UpdateOrderDetailPriceAsync(id, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteOrderDetail([FromRoute] int id)
        {
            try
            {
                await _orderDetailService.DeleteOrderDetailAsync(id);
                return Ok(new { message = "Order detail deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
       
    }
}
