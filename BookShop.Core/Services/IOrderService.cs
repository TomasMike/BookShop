using BookShop.Core.Models;

namespace BookShop.Core.Services
{
    public interface IOrderService
    {
        Task<(Order Order, bool Created)> AddBookToOpenOrderAsync(int bookId, int customerId);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId);
        Task<Order?> UpdateOrderDetailQuantityAsync(int orderId, int detailId, int customerId, int quantity);
        Task<Order?> RemoveOrderDetailAsync(int orderId, int detailId, int customerId);
        Task<bool> CancelOrderAsync(int orderId);
        Task<Order> ConfirmOrderAsync(int orderId);
    }
}