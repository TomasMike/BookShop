using BookShop.API.DTOs;
using BookShop.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookShop.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerById(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return NotFound(new { error = "Customer not found" });

            var dto = MapToCustomerDto(customer);
            return Ok(dto);
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<CustomerDto>> GetCustomerByEmail( string email)
        {
            var customer = await _customerService.GetCustomerByEmailAsync(email);
            if (customer == null)
                return NotFound(new { error = "Customer not found" });

            var dto = MapToCustomerDto(customer);
            return Ok(dto);
        }

        private static CustomerDto MapToCustomerDto(BookShop.Core.Models.Customer customer)
        {
            return new CustomerDto
            {
                Id = customer.Id,
                Email = customer.Email,
                PhoneNumber = customer.PhoneNumber,
                UserId = customer.UserId
            };
        }
    }
}


