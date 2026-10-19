using BookShop.Core.Models;

namespace BookShop.API.DTOs
{
    public class CreateOrderRequest
    {
        public int BookId { get; set; }
    }

    /// <summary>Provides the new quantity for a book in an open order.</summary>
    public class UpdateOrderDetailRequest
    {
        public int Quantity { get; set; }
    }

    /// <summary>Represents one book and its quantity in an order.</summary>
    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    /// <summary>Represents an order and its book details.</summary>
    public class OrderDto
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public List<OrderDetailDto> Details { get; set; } = [];
    }

    public class  CustomerDto
    {
        public int Id { get; set; }
        public  string UserId { get; set; } = string.Empty;
        public  string Email { get; set; } = string.Empty;
        public  string PhoneNumber { get; set; } = string.Empty;
    }

    public class ProcessPaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class PaymentDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? TransactionId { get; set; }
    }
}
