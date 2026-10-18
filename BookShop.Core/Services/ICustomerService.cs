using BookShop.Core.Models;

namespace BookShop.Core.Services
{
    public interface ICustomerService
    {
        Task<Customer> CreateCustomerAsync(string userId, string email, string phoneNumber = "");
        Task<Customer?> GetCustomerByUserIdAsync(string userId);
        Task<Customer?> GetCustomerByIdAsync(int customerId);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task UpdateCustomerAsync(Customer customer);
        Task DeleteCustomerAsync(int customerId);
    }
}
