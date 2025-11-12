using AnalyticRepository.Model.DTO;
using AnalyticService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionController : ControllerBase
    {
        private readonly IPredictionService _predictionService;

        public PredictionController(IPredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        /// <summary>
        /// Chạy dự đoán nhu cầu cho một hoặc nhiều kế hoạch.
        /// </summary>
        /// <param name="requests">Danh sách các kế hoạch (Vehicle, Agency, Price...) cho tháng tới.</param>
        [HttpPost("demand")]
        [ProducesResponseType(typeof(List<PredictionResponseDto>), 200)]
        public async Task<IActionResult> PredictDemand([FromBody] List<PredictDemandDto> requests)
        {
            if (requests == null || !requests.Any())
            {
                return BadRequest("Request body không được rỗng.");
            }

            try
            {
                var predictions = await _predictionService.PredictAsync(requests);
                return Ok(predictions);
            }
            catch (Exception ex)
            {
                // Trả về lỗi 500 nếu model fail
                return StatusCode(500, $"Lỗi máy chủ nội bộ khi dự đoán: {ex.Message}");
            }
        }
    }
}
