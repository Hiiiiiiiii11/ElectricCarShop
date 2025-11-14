using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.OrderDTO;
using OrderService.Services;

namespace OrderAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CustomerRequest request)
        {

            try
            {
                var createdCustomer = await _customerService.FindOrCreateCustomerAsync(request);
                return Ok(createdCustomer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateCustomer(int id, [FromForm] CustomerUpdateRequest request)
        {

            try
            {
                var updatedCustomer = await _customerService.UpdateAsync(id, request);
                return Ok(updatedCustomer);
            }
            catch (KeyNotFoundException knfEx)
            {
                return NotFound(new { message = knfEx.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            try
            {
                var customers = await _customerService.GetAllAsync();
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            try
            {
                var customer = await _customerService.GetByIdAsync(id);
                if (customer == null)
                    return NotFound(new { message = $"Customer with ID {id} not found." });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteAsync(id);
                if (!result)
                    return NotFound(new { message = $"Customer with ID {id} not found." });

                return Ok(new { message = $"Customer with ID {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("by-email")]
        public async Task<IActionResult> GetCustomerByEmail([FromQuery] string email)
        {
            try
            {
                var customer = await _customerService.GetByEmailAsync(email);
                if (customer == null)
                    return NotFound(new { message = $"Customer with Email {email} not found." });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("by-phone")]
        public async Task<IActionResult> GetCustomerByPhone([FromQuery] string phone)
        {
            try
            {
                var customer = await _customerService.GetByPhoneAsync(phone);
                if (customer == null)
                    return NotFound(new { message = $"Customer with Phone {phone} not found." });

                return Ok(customer);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }

        [HttpGet("search-by-name")]
        public async Task<IActionResult> SearchCustomersByName([FromQuery] string name)
        {
            try
            {
                var customers = await _customerService.SearchByNameAsync(name);
                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Internal server error: " + ex.Message });
            }
        }
    }
}
