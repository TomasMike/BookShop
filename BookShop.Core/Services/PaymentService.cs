using BookShop.Core.Models;
using BookShop.Core.Repositories;

namespace BookShop.Core.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IOrderRepository _orderRepository;

        public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository)
        {
            _paymentRepository = paymentRepository;
            _orderRepository = orderRepository;
        }

        public async Task<Payment> ProcessPaymentAsync(int orderId, decimal amount, PaymentMethod method)
        {
            var order = await _orderRepository.GetByIdAsync(orderId);
            if (order == null)
                throw new InvalidOperationException("Order not found");

            if (amount != order.TotalPrice)
                throw new InvalidOperationException("Payment amount does not match order total");

            if (order.Status != OrderStatus.Confirmed && order.Status != OrderStatus.Pending)
                throw new InvalidOperationException("Order is not eligible for payment");

            var payment = new Payment
            {
                Order = order,
                OrderId = orderId,
                Amount = amount,
                PaymentMethod = method,
                PaymentDate = DateTime.UtcNow,
                Status = PaymentStatus.Pending,
                TransactionId = GenerateTransactionId()
            };

            // Process payment with external provider
            var isSuccessful = await ProcessWithPaymentGatewayAsync(payment);

            if (!isSuccessful)
            {
                payment.Status = PaymentStatus.Failed;
            }
            else
            {
                payment.Status = PaymentStatus.Completed;
                order.Status = OrderStatus.Paid;
                await _orderRepository.UpdateAsync(order);
            }

            return await _paymentRepository.AddAsync(payment);
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _paymentRepository.GetByIdAsync(paymentId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderAsync(int orderId)
        {
            return await _paymentRepository.GetByOrderIdAsync(orderId);
        }

        public async Task<bool> RefundPaymentAsync(int paymentId)
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId);
            if (payment == null)
                return false;

            if (payment.Status != PaymentStatus.Completed)
                throw new InvalidOperationException("Only completed payments can be refunded");

            payment.Status = PaymentStatus.Refunded;
            await _paymentRepository.UpdateAsync(payment);

            var order = await _orderRepository.GetByIdAsync(payment.OrderId);
            if (order != null)
            {
                order.Status = OrderStatus.Cancelled;
                await _orderRepository.UpdateAsync(order);
            }

            return true;
        }

        private async Task<bool> ProcessWithPaymentGatewayAsync(Payment payment)
        {
            // Simulate payment gateway processing
            // In production, integrate with Stripe, PayPal, or another provider
            await Task.Delay(100); // Simulate processing time
            return new Random().Next(0, 100) > 5; // 95% success rate for demo
        }

        private string GenerateTransactionId()
        {
            return $"TXN-{DateTime.UtcNow.Ticks}-{Guid.NewGuid().ToString().Substring(0, 8)}";
        }
    }
}
