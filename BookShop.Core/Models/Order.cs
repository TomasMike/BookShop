namespace BookShop.Core.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int BookId { get; set; }
        public required Book Book { get; set; }
        public int CustomerId { get; set; }
        public required Customer Customer { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalPrice { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}




