using BookShop.Core.Models;

namespace BookShop.Core.Services
{
    public interface IPaymentService
    {
        Task<Payment> ProcessPaymentAsync(int orderId, decimal amount, PaymentMethod method);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(int orderId);
        Task<bool> RefundPaymentAsync(int paymentId);
    }
}
