namespace BookShop.Core.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public required string UserId { get; set; }
        public required string Email { get; set; }
        public required string PhoneNumber { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
