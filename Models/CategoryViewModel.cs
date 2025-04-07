namespace CafeMenuWebApp.Models
{
    public class CategoryViewModel
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? ParentCategoryId { get; set; }
        public string ParentCategoryName { get; set; }
        public int TenantId { get; set; }
        public IFormFile ImageFile { get; set; }
    }
}
