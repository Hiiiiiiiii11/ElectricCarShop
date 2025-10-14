using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FeedbackController : Controller
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService = feedbackService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllFeedbacks()
        {
            var feedbacks = await _feedbackService.GetAllAsync();
            return Ok(feedbacks);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFeedbackById(int id)
        {
            var feedback = await _feedbackService.GetByIdAsync(id);
            if (feedback == null)
                return NotFound($"Feedback with ID {id} not found.");
            return Ok(feedback);
        }
        [HttpGet("customer/{customerId}")]
        public async Task<IActionResult> GetFeedbacksByCustomerId(int customerId)
        {
            var feedbacks = await _feedbackService.GetByCustomerIdAsync(customerId);
            return Ok(feedbacks);
        }
        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetFeedbacksByStatus(string status)
        {
            var feedbacks = await _feedbackService.GetByStatusAsync(status);
            return Ok(feedbacks);
        }
        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromBody] FeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdFeedback = await _feedbackService.CreateAsync(request);
                return Ok(createdFeedback);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateFeedback(int id, [FromBody] FeedbackRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedFeedback = await _feedbackService.UpdateAsync(id, request);
                return Ok(updatedFeedback);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(knfEx.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFeedback(int id)
        {
            try
            {
                var deleted = await _feedbackService.DeleteAsync(id);
                if (!deleted)
                    return NotFound($"Feedback with ID {id} not found.");
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

    }
}
