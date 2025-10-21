using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDriveController : ControllerBase
    {
        private readonly ITestDriveService _testDriveService;

        public TestDriveController(ITestDriveService testDriveService)
        {
            _testDriveService = testDriveService;
        }

        // ================= GET BY ID =================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTestDriveById(int id)
        {
            try
            {
                var testDrive = await _testDriveService.GetTestDriveByIdAsync(id);
                if (testDrive == null)
                {
                    return NotFound(new { message = $"Test drive with ID {id} not found." });
                }
                return Ok(testDrive);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while retrieving the test drive: {ex.Message}" });
            }
        }

        // ================= GET ALL =================
        [HttpGet]
        public async Task<IActionResult> GetAllTestDrives()
        {
            try
            {
                var testDrives = await _testDriveService.GetAllTestDrivesAsync();
                return Ok(testDrives);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"An error occurred while retrieving test drives: {ex.Message}" });
            }
        }

        // ================= CREATE =================
        [HttpPost]
        public async Task<IActionResult> CreateTestDrive([FromBody] CreateTestDriveRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var createdTestDrive = await _testDriveService.CreateTestDriveAsync(request);
                return Ok(createdTestDrive);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error creating test drive: {ex.Message}" });
            }
        }

        // ================= UPDATE =================
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTestDrive(int id, [FromBody] UpdateTestDriveRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var updatedTestDrive = await _testDriveService.UpdateTestDriveAsync(id, request);
                return Ok(updatedTestDrive);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error updating test drive: {ex.Message}" });
            }
        }

        // ================= DELETE =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestDrive(int id)
        {
            try
            {
                var result = await _testDriveService.DeleteTestDriveAsync(id);
                if (!result)
                {
                    return NotFound(new { message = $"Test drive with ID {id} not found." });
                }

                return Ok(new { message = "Test drive deleted successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error deleting test drive: {ex.Message}" });
            }
        }
    }
}
