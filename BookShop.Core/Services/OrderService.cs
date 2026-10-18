using BookShop.Core.Models;
using BookShop.Core.Repositories;

namespace BookShop.Core.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrderService(IOrderRepository orderRepository, IBookRepository bookRepository, ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _bookRepository = bookRepository;
            _customerRepository = customerRepository;
        }

        public async Task<Order> CreateOrderAsync(int bookId, int customerId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
                throw new InvalidOperationException("Book not found");

            if (!book.IsAvailable)
                throw new InvalidOperationException("Book is not available for order");

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            var order = new Order
            {
                BookId = bookId,
                CustomerId = customerId,
                Book = book,
                Customer = null!,            // quick workaround to satisfy 'required'
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalPrice = book.Price
            };

            return await _orderRepository.AddAsync(order);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _orderRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                return false;

            if (order.Status == OrderStatus.Paid || order.Status == OrderStatus.Shipped)
                throw new InvalidOperationException("Cannot cancel a paid or shipped order");

            order.Status = OrderStatus.Cancelled;
            await _orderRepository.UpdateAsync(order);
            return true;
        }

        public async Task<Order> ConfirmOrderAsync(int orderId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            order.Status = OrderStatus.Confirmed;
            await _orderRepository.UpdateAsync(order);
            return order;
        }
    }
}