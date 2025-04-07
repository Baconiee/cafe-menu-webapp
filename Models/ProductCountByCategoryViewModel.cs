using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace CafeMenuWebApp.Models
{
    public class ProductCountByCategoryViewModel
    {
        public string CategoryName { get; set; }

        public int ProductCount { get; set; }

        public bool HasSubCategories { get; set; }

        public List<ProductCountByCategoryViewModel> SubCategories { get; set; } = new List<ProductCountByCategoryViewModel>();
    }
}
