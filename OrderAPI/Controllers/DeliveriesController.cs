using Microsoft.AspNetCore.Mvc;
using OrderAPI.Controllers.Binding;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DeliveriesController : ControllerBase
    {
        private readonly IDeliveryService _deliveryService;
        public DeliveriesController(IDeliveryService deliveryService)
        {
            _deliveryService = deliveryService;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] DeliveryCreateRequest request)
        {
            try
            {
                var result = await _deliveryService.CreateAsync(request);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var result = await _deliveryService.GetByIdAsync(id);
                if (result == null)
                    return NotFound(new { message = "Delivery not found." });
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpGet("by-order/{orderId}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            try
            {
                var result = await _deliveryService.GetByOrderIdAsync(orderId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet("agency/{agencyId}")]
        public async Task<IActionResult> GetByAgencyId(int agencyId)
        {
            try
            {
                var result = await _deliveryService.GetByAgencyAsync(agencyId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> List(
            [FromQuery] string? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var (items, total) = await _deliveryService.ListAsync(status, from, to, page, pageSize);
                return Ok(new { items, total });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromForm] DeliveryUpdateRequest request)
        {
            try
            {
                var result = await _deliveryService.UpdateAsync(id, request);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _deliveryService.DeleteAsync(id);
                return Ok(new {message =$"Delete delivery with id {id} success"});
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }




    }
}
