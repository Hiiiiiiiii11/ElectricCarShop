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

        // GET: api/orderdetail/order/123
        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrder(int orderId)
        {
            try
            {
                var details = await _orderDetailService.GetByOrderAsync(orderId);
                if (details == null || !details.Any())
                    return NotFound(new { message = $"No order details found for order ID {orderId}." });

                return Ok(details);
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

        // POST: api/orderdetail
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateOrderDetailRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is null." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var detail = await _orderDetailService.AddAsync(request.OrderId, request.QuotationId, request.UnitPrice);
                // Sau khi AddAsync, service đã recalc; gọi lại để chắc chắn lấy tổng mới
                var newTotal = await _orderDetailService.RecalculateOrderTotalAsync(request.OrderId);

                return Ok(new
                {
                    message = "Order detail created.",
                    detail,
                    orderId = request.OrderId,
                    orderTotal = newTotal
                });
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

        // PUT: api/orderdetail/456/price
        [HttpPut("{detailId:int}/price")]
        public async Task<IActionResult> UpdatePrice(int detailId, [FromBody] UpdateOrderDetailPriceRequest request)
        {
            try
            {
                if (request == null)
                    return BadRequest(new { message = "Request body is null." });

                if (!ModelState.IsValid)
                    return BadRequest(ModelState);

                var ok = await _orderDetailService.UpdateUnitPriceAsync(detailId, request.NewUnitPrice);
                return Ok(new { message = ok ? "Unit price updated." : "No changes applied." });
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

        // DELETE: api/orderdetail/456
        [HttpDelete("{detailId:int}")]
        public async Task<IActionResult> Delete(int detailId)
        {
            try
            {
                var ok = await _orderDetailService.RemoveAsync(detailId);
                return Ok(new { message = ok ? $"Order detail {detailId} deleted." : "No changes applied." });
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

        // POST: api/orderdetail/order/123/recalc
        [HttpPost("order/{orderId:int}/recalc")]
        public async Task<IActionResult> RecalculateOrderTotal(int orderId)
        {
            try
            {
                var total = await _orderDetailService.RecalculateOrderTotalAsync(orderId);
                return Ok(new { orderId, orderTotal = total });
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
    }
}
