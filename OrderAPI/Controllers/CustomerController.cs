using Microsoft.AspNetCore.Mvc;
using OrderRepository.Model.Request;
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
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var createdCustomer = await _customerService.CreateAsync(request);
                return Ok(createdCustomer);
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            try
            {
                var updatedCustomer = await _customerService.UpdateAsync(id, request);
                return Ok(updatedCustomer);
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
        [HttpGet]
        public async Task<IActionResult> GetAllCustomers()
        {
            var customers = await _customerService.GetAllAsync();
            return Ok(customers);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerService.GetByIdAsync(id);
            if (customer == null)
            {
                return NotFound($"Customer with ID {id} not found.");
            }
            return Ok(customer);
        }
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                var result = await _customerService.DeleteAsync(id);
                if (!result)
                {
                    return NotFound($"Customer with ID {id} not found.");
                }
                return Ok(new { Message = $"Customer with ID {id} deleted successfully." });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        // Extra features
        [HttpGet("by-email")]
        public async Task<IActionResult> GetCustomerByEmail([FromQuery] string email)
        {
            var customer = await _customerService.GetByEmailAsync(email);
            if (customer == null)
            {
                return NotFound($"Customer with Email {email} not found.");
            }
            return Ok(customer);
        }
        [HttpGet("by-phone")]
        public async Task<IActionResult> GetCustomerByPhone([FromQuery] string phone)
        {
            var customer = await _customerService.GetByPhoneAsync(phone);
            if (customer == null)
            {
                return NotFound($"Customer with Phone {phone} not found.");
            }
            return Ok(customer);
        }
        [HttpGet("search-by-name")]
        public async Task<IActionResult> SearchCustomersByName([FromQuery] string name)
        {
            var customers = await _customerService.SearchByNameAsync(name);
            return Ok(customers);
        }

    }
}
