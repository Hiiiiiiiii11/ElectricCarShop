using AgencyRepository.Model.DTO;
using AgencyService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AllocationAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestDriveController : Controller
    {
        private readonly ITestDriveService _testDriveService;
        public TestDriveController(ITestDriveService testDriveService)
        {
            _testDriveService = testDriveService;
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTestDriveById(int id)
        {
            var testDrive = await _testDriveService.GetTestDriveByIdAsync(id);
            if (testDrive == null)
            {
                return NotFound();
            }
            return Ok(testDrive);
        }
        [HttpGet]
        public async Task<IActionResult> GetAllTestDrives()
        {
            var testDrives = await _testDriveService.GetAllTestDrivesAsync();
            return Ok(testDrives);
        }
        [HttpPost]
        public async Task<IActionResult> CreateTestDrive([FromBody] CreateTestDriveRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdTestDrive = await _testDriveService.CreateTestDriveAsync(request);
                return Ok(createdTestDrive);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });

            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTestDrive(int id, [FromBody] UpdateTestDriveRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedTestDrive = await _testDriveService.UpdateTestDriveAsync(id, request);
                return Ok(updatedTestDrive);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTestDrive(int id)
        {
            try
            {
                var result = await _testDriveService.DeleteTestDriveAsync(id);
                if (!result)
                {
                    return NotFound($"Test drive is not found");
                }
                return Ok(new { message = "Test drive deleted successfully." });
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        

    }
}
