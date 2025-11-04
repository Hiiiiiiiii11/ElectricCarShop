using AllocationRepository.Model.DTO;
using AllocationService.Services;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VehiclePromotionController : Controller
    {
        private readonly IVehiclePromotionService _vehiclePromotionService;
        public VehiclePromotionController(IVehiclePromotionService vehiclePromotionService)
        {
            _vehiclePromotionService = vehiclePromotionService;
        }
        [HttpPost]
        public async Task<IActionResult> CreateVehiclePromotion([FromForm] VehiclePromotionRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var createdPromotion = await _vehiclePromotionService.CreateAsync(request);
                return Ok(createdPromotion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehiclePromotion(int id, [FromForm] VehiclePromotionUpdateRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            try
            {
                var updatedPromotion = await _vehiclePromotionService.UpdateAsync(id, request);
                return Ok(updatedPromotion);
            }
            catch (KeyNotFoundException exception)
            {
                return NotFound(new { message = exception.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehiclePromotion(int id)
        {
            try
            {
                var result = await _vehiclePromotionService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = $"Vehicle promotion with id {id} not found." });
                return Ok(new { Message = $"Delete promotion with id {id} successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehiclePromotionById(int id)
        {
            try
            {
                var promotion = await _vehiclePromotionService.GetByIdAsync(id);
                if (promotion == null)
                    return NotFound(new { message = $"Vehicle promotion with id {id} not found." });
                return Ok(promotion);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("agecy/{agencyId}")]
        public async Task<IActionResult> GetPromotionsByAgencyId(int agencyId)
        {
            try
            {
                var promotions = await _vehiclePromotionService.GetPromotionByAgencyIdAsync(agencyId);
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetAllVehiclePromotions()
        {
            try
            {
                var promotions = await _vehiclePromotionService.GetAllAsync();
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("vehicle/{vehicleId}")]
        public async Task<IActionResult> GetPromotionsByVehicleId(int vehicleId)
        {
            try
            {
                var promotions = await _vehiclePromotionService.GetPromotionsByVehicleIdAsync(vehicleId);
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("active")]
        public async Task<IActionResult> GetActivePromotions()
        {
            try
            {
                var promotions = await _vehiclePromotionService.GetActivePromotionsAsync();
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        [Route("expired")]
        public async Task<IActionResult> GetExpiredPromotions()
        {
            try
            {
                var promotions = await _vehiclePromotionService.GetExpiredPromotionsAsync();
                return Ok(promotions);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
