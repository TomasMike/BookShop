using BookShop.Core.Models;

namespace BookShop.Core.Repositories
{
    public interface IPaymentRepository
    {
        Task<Payment> AddAsync(Payment payment);
        Task<Payment?> GetByIdAsync(int id);
        Task<IEnumerable<Payment>> GetByOrderIdAsync(int orderId);
        Task<IEnumerable<Payment>> GetAllAsync();
        Task UpdateAsync(Payment payment);
        Task DeleteAsync(int id);
    }
}
