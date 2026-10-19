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

        public async Task<(Order Order, bool Created)> AddBookToOpenOrderAsync(int bookId, int customerId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
                throw new InvalidOperationException("Book not found");

            if (!book.IsAvailable)
                throw new InvalidOperationException("Book is not available for order");

            var customer = await _customerRepository.GetByIdAsync(customerId);
            if (customer == null)
                throw new InvalidOperationException("Customer not found");

            var order = await _orderRepository.GetLatestOpenByCustomerIdAsync(customerId);
            if (order == null)
            {
                order = new Order
                {
                    BookId = bookId,
                    CustomerId = customerId,
                    Book = book,
                    Customer = customer,
                    OrderDate = DateTime.UtcNow,
                    Status = OrderStatus.Pending,
                    TotalPrice = 0
                };
                AddBookDetail(order, book);
                return (await _orderRepository.AddAsync(order), true);
            }

            AddBookDetail(order, book);
            await _orderRepository.UpdateAsync(order);
            return (order, false);
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _orderRepository.GetByIdAsync(orderId);
        }

        public async Task<IEnumerable<Order>> GetOrdersByCustomerAsync(int customerId)
        {
            return await _orderRepository.GetByCustomerIdAsync(customerId);
        }

        public async Task<Order?> UpdateOrderDetailQuantityAsync(int orderId, int detailId, int customerId, int quantity)
        {
            if (quantity < 1)
                throw new InvalidOperationException("Quantity must be at least one");

            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.CustomerId != customerId)
                return null;
            EnsureOrderIsOpen(order);

            var detail = order.Details.FirstOrDefault(item => item.Id == detailId);
            if (detail == null)
                return null;

            detail.Quantity = quantity;
            UpdateTotalPrice(order);
            await _orderRepository.UpdateAsync(order);
            return order;
        }

        public async Task<Order?> RemoveOrderDetailAsync(int orderId, int detailId, int customerId)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null || order.CustomerId != customerId)
                return null;
            EnsureOrderIsOpen(order);

            var detail = order.Details.FirstOrDefault(item => item.Id == detailId);
            if (detail == null)
                return null;

            order.Details.Remove(detail);
            UpdateTotalPrice(order);
            await _orderRepository.UpdateAsync(order);
            return order;
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

        private static void AddBookDetail(Order order, Book book)
        {
            var detail = order.Details.FirstOrDefault(item => item.BookId == book.Id);
            if (detail == null)
            {
                order.Details.Add(new OrderDetail
                {
                    Order = order,
                    BookId = book.Id,
                    Book = book,
                    Quantity = 1,
                    UnitPrice = book.Price
                });
            }
            else
            {
                detail.Quantity++;
            }

            UpdateTotalPrice(order);
        }

        private static void UpdateTotalPrice(Order order)
        {
            order.TotalPrice = order.Details.Sum(detail => detail.UnitPrice * detail.Quantity);
        }

        private static void EnsureOrderIsOpen(Order order)
        {
            if (order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Only open orders can be changed");
        }
    }
}