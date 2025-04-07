namespace CafeMenuWebApp.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }
        public string CategoryName { get; set; }
        public int CategoryId { get; set; }
        public List<string> Properties { get; set; } = new List<string>();
    }
}
