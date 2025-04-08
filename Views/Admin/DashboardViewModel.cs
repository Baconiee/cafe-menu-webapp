using CafeMenuWebApp.Models;

namespace CafeMenuWebApp.Views.Admin
{
    public class DashboardViewModel
    {
        public List<ProductCountByCategoryViewModel> ProductCounts { get; set; }
        public decimal CurrencyRate { get; set; }
    }
}
