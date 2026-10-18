using BookShop.Core.Models;
using BookShop.Core.Repositories;

namespace BookShop.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> CreateCustomerAsync(string userId, string email, string phoneNumber = "")
        {
            // Check if customer already exists for this user
            var existingCustomer = await GetCustomerByUserIdAsync(userId);
            if (existingCustomer != null)
                throw new InvalidOperationException("Customer already exists for this user");

            var customer = new Customer
            {
                UserId = userId,
                Email = email,
                PhoneNumber = phoneNumber
            };

            return await _customerRepository.AddAsync(customer);
        }

        public async Task<Customer?> GetCustomerByUserIdAsync(string userId)
        {
            return await _customerRepository.GetByUserIdAsync(userId);
        }

        public async Task<Customer?> GetCustomerByIdAsync(int customerId)
        {
            return await _customerRepository.GetByIdAsync(customerId);
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _customerRepository.GetAllAsync();
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            await _customerRepository.UpdateAsync(customer);
        }

        public async Task DeleteCustomerAsync(int customerId)
        {
            await _customerRepository.DeleteAsync(customerId);
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _customerRepository.GetByEmailAsync(email);
        }
    }
}
