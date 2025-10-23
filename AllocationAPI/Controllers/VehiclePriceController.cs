using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclePriceController : Controller
    {
        private readonly IVehiclePriceService _vehiclePriceService;
        public VehiclePriceController(IVehiclePriceService vehiclePriceService)
        {
            _vehiclePriceService = vehiclePriceService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateVehiclePrice([FromForm] VehiclePriceRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var createdPrice = await _vehiclePriceService.CreateAsync(request);
                return Ok(createdPrice);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
           
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehiclePrice(int id, [FromForm] VehiclePriceUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var updatedPrice = await _vehiclePriceService.UpdateAsync(id, request);
                return Ok(updatedPrice);
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
        public async Task<IActionResult> DeleteVehiclePrice(int id)
        {
            try
            {
                var result = await _vehiclePriceService.DeleteAsync(id);
                if (!result)
                    return NotFound($"Vehicle price with id {id} not found.");
                return Ok(new {Message = $"Delete price with id {id} successfully"});
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehiclePriceById(int id)
        {
            try
            {
                var price = await _vehiclePriceService.GetByIdAsync(id);
                if (price == null)
                    return NotFound($"Vehicle price with id {id} not found.");
                return Ok(price);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllVehiclePrices()
        {
            try
            {
                var prices = await _vehiclePriceService.GetAllAsync();
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        // 3 hàm mở rộng
        [HttpGet("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetPricesByVehicleId(int vehicleId)
        {
            try
            {
                var prices = await _vehiclePriceService.GetPricesByVehicleIdAsync(vehicleId);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        //[HttpGet("vehicle/{vehicleId}/history")]
        //public async Task<IActionResult> GetPriceHistory(int vehicleId)
        //{
        //    try
        //    {
        //        var prices = await _vehiclePriceService.GetPriceHistoryAsync(vehicleId);
        //        return Ok(prices);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, $"Internal server error: {ex.Message}");
        //    }
        //}
        [HttpGet("agency/{agencyId}")]
        public async Task<IActionResult> GetPricesByAgency(int agencyId)
        {
            try
            {
                var prices = await _vehiclePriceService.GetPricesByAgencyAsync(agencyId);
                return Ok(prices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
