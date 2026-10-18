namespace BookShop.Core.Models
{
    public class Book
    {
        public int Id { get; set; }
        public required decimal Price { get; set; }
        public required Author Author { get; set; }
        public required string Title { get; set; }
        public required int PublishedYear { get; set; }
        public required bool IsAvailable { get; set; }
        public required string ISBN { get; set; }
    }
}


    

   
