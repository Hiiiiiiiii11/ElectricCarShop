using AllocationService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderAPIService.Services;
using OrderRepository.Model.OrderDTO;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuotationController : ControllerBase
    {
        private readonly IQuotationService _quotationService;

        public QuotationController(IQuotationService quotationService)
        {
            _quotationService = quotationService;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllQuotations()
        {
            try
            {
                var quotations = await _quotationService.GetAllQuotationsAsync();
                return Ok(quotations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuotationById(int id)
        {
            try
            {

                var quotation = await _quotationService.GetQuotationByIdAsync(id);
                if (quotation == null)
                    return NotFound(new { message = $"Quotation with ID {id} not found." });

                return Ok(quotation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuotation([FromForm] CreateQuotationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data." });
                var userIdClaim = User.FindFirst("id")?.Value;
                if (userIdClaim != null)
                    request.CreateBy = int.Parse(userIdClaim);
                var newQuotation = await _quotationService.CreateQuotationAsync(request);
                return Ok(newQuotation);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuotation(int id, [FromForm] UpdateQuotationRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Invalid request data." });

                var updatedQuotation = await _quotationService.UpdateQuotationAsync(id, request);
                return Ok(updatedQuotation);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuotation(int id)
        {
            try
            {
                var result = await _quotationService.DeleteQuotationAsync(id);
                if (!result)
                    return NotFound(new { message = $"Quotation with ID {id} not found." });

                return Ok(new { message = "Quotation deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
        [HttpGet("createby/{userId}")]
        public async Task<IActionResult> GetQuotationCreateByUser(int userId)
        {
            try
            {
                var quotations = await _quotationService.GetQuotationByUserCreateId(userId);
                return Ok(quotations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }

        }
        [HttpGet("agency/{agencyId}")]
        public async Task<IActionResult> GetQuotationByAgencyId(int agencyId)
        {
            try
            {
                var quotations = await _quotationService.GetQuotationByAgencyId(agencyId);
                return Ok(quotations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }
    }
}
