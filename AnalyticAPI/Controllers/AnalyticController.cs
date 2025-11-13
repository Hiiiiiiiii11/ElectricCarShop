using AnalyticRepository.Model.DTO;
using AnalyticService.Services;
using GrpcService;
using Microsoft.AspNetCore.Mvc;

namespace AnalyticAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticController : ControllerBase
    {
        private readonly IAnalyticService _analyticsService;
        private readonly ILogger<AnalyticController> _logger;

        public AnalyticController(IAnalyticService analyticsService, ILogger<AnalyticController> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        /// <summary>
        /// Lấy trạng thái của dịch vụ ETL (Worker)
        /// </summary>
        [HttpGet("status")]
        [ProducesResponseType(typeof(ETLStatusResponse), 200)]
        public async Task<IActionResult> GetEtlStatus()
        {
            var status = await _analyticsService.GetEtlStatusAsync();
            return Ok(status);
        }

        /// <summary>
        /// Kích hoạt thủ công 1 lần chạy ETL
        /// </summary>
        [HttpPost("trigger")]
        [ProducesResponseType(typeof(object), 202)]
        public IActionResult TriggerEtl()
        {
            _logger.LogInformation("API endpoint /trigger was called.");
            _analyticsService.TriggerEtlManually();
            return Accepted(new { message = "ETL process triggered. It will start on the next worker cycle (within 10 minutes)." });
        }

        /// <summary>
        /// (Quan trọng) Xuất dữ liệu đã được tổng hợp để cho AI huấn luyện
        /// </summary>
        /// <param name="startYear">Năm bắt đầu (ví dụ: 2024)</param>
        /// <param name="startMonth">Tháng bắt đầu (ví dụ: 1)</param>
        /// <param name="endYear">Năm kết thúc (ví dụ: 2025)</param>
        /// <param name="endMonth">Tháng kết thúc (ví dụ: 10)</param>
        [HttpGet("data")]
        [ProducesResponseType(typeof(IEnumerable<DemandFeatureResponse>), 200)]
        public async Task<IActionResult> GetAggregatedData(
            [FromQuery] int startYear,
            [FromQuery] int startMonth,
            [FromQuery] int endYear,
            [FromQuery] int endMonth,
            [FromQuery] int? vehicleId,
            [FromQuery] int? agencyId
            )
        {
            if (startYear == 0 || startMonth == 0 || endYear == 0 || endMonth == 0)
            {
                var now = DateTime.UtcNow;
                return BadRequest("Vui lòng cung cấp startYear, startMonth, endYear, endMonth. " +
                                  $"Ví dụ: ?startYear={now.Year - 1}&startMonth=1&endYear={now.Year}&endMonth={now.Month}");
            }

            var data = await _analyticsService.GetDemandFeaturesAsync(startYear, startMonth, endYear, endMonth, vehicleId, agencyId);
            return Ok(data);
        }

        [HttpGet("orders")]
        [ProducesResponseType(typeof(IEnumerable<OrderReply>), 200)]
        public async Task<IActionResult> GetOrders()
        {
            try
            {
                var data = await _analyticsService.GetAllOrdersAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

        [HttpGet("agencies")]
        [ProducesResponseType(typeof(IEnumerable<AgencyReply>), 200)]
        public async Task<IActionResult> GetAgencies()
        {
            try
            {
                var data = await _analyticsService.GetAllAgencysAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

        [HttpGet("vehicle")]
        [ProducesResponseType(typeof(IEnumerable<VehicleReply>), 200)]
        public async Task<IActionResult> GetVehicles()
        {
            try
            {
                var data = await _analyticsService.GetAllVehiclesAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }
        [HttpGet("vehicle-instance")]
        [ProducesResponseType(typeof(IEnumerable<VehicleInstanceReply>), 200)]
        public async Task<IActionResult> GetVehicleInstances()
        {
            try
            {
                var data = await _analyticsService.GetAllVehicleInstancesAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

        [HttpGet("quotations")]
        [ProducesResponseType(typeof(IEnumerable<QuotationReply>), 200)]
        public async Task<IActionResult> GetQuotations()
        {
            
            try
            {
                var data = await _analyticsService.GetAllQuotationsAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

        [HttpGet("vehicle-prices")]
        [ProducesResponseType(typeof(IEnumerable<VehiclePriceReply>), 200)]
        public async Task<IActionResult> GetVehiclePrices()
        {
            
            try
            {
                var data = await _analyticsService.GetAllVehiclePricesAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

        [HttpGet("promotions")]
        [ProducesResponseType(typeof(IEnumerable<VehiclePromotionReply>), 200)]
        public async Task<IActionResult> GetPromotions()
        {
            
            try
            {
                var data = await _analyticsService.GetAllPromotionsAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

        [HttpGet("test-drives")]
        [ProducesResponseType(typeof(IEnumerable<TestDriveReply>), 200)]
        public async Task<IActionResult> GetTestDrives()
        {
            
            try
            {
                var data = await _analyticsService.GetAllTestDrivesAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

        [HttpGet("agency-targets")]
        [ProducesResponseType(typeof(IEnumerable<AgencyTargetReply>), 200)]
        public async Task<IActionResult> GetAgencyTargets()
        {
            
            try
            {
                var data = await _analyticsService.GetAllAgencyTargetsAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });

            }
        }

    }
}
