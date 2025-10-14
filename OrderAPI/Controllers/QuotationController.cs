using AllocationService.Services;
using Microsoft.AspNetCore.Mvc;
using OrderAPIService.Services;
using OrderRepository.Model.Request;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuotationById(int id)
        {
            try
            {
                var quotation = await _quotationService.GetQuotationByIdAsync(id);
                if (quotation == null)
                {
                    return NotFound($"Quotation with ID {id} not found.");
                }
                return Ok(quotation);
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new { message = "An internal server error occurred.", error = ex.Message });

            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuotation([FromBody] CreateQuotationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var newQuotation = await _quotationService.CreateQuotationAsync(request);
                return Ok(newQuotation);
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new { message = "An internal server error occurred.", error = ex.Message });

            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuotation(int id, [FromBody] UpdateQuotationRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var updatedQuotation = await _quotationService.UpdateQuotationAsync(id, request);
                return Ok(updatedQuotation);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new { message = "An internal server error occurred.", error = ex.Message });

            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuotation(int id)
        {
            try
            {
                var result = await _quotationService.DeleteQuotationAsync(id);
                if (!result)
                {
                    return NotFound($"Quotation with ID {id} not found.");
                }
                return Ok(new { message = "Delete quotation success" }); 
            }
            catch (Exception ex)
            {
                // Log the exception details here
                return StatusCode(500, new { message = "An internal server error occurred.", error = ex.Message });

            }
        }
    }
}
