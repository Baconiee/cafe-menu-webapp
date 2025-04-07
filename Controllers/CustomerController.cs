using CafeMenuWebApp.Data;
using CafeMenuWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;


namespace CafeMenuWebApp.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IMemoryCache _cache;
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomerController(ApplicationDbContext context, IMemoryCache cache, IHttpClientFactory httpClientFactory)
        {
            _context = context;
            _cache = cache;
            _httpClientFactory = httpClientFactory;
        }

        public int TenantId
        {
            get
            {
                string TenantIdValue = User.Claims.FirstOrDefault(c => c.Type == "TenantId")?.Value;
                if (string.IsNullOrEmpty(TenantIdValue))
                {
                    throw new InvalidOperationException("TenantId not found in user claims.");
                }
                return int.Parse(TenantIdValue);
            }
        }

        private async Task<decimal> GetCurrencyRateAsync()
        {
            const string cacheKey = "USDtoTRYRate";
            if (!_cache.TryGetValue(cacheKey, out decimal exchangeRate))
            {
                var client = _httpClientFactory.CreateClient();
                string apiKey = "26e9b072bfe042128c6410fb8";
                string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}&base=USD";

                try
                {
                    var response = await client.GetStringAsync(url);
                    var json = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
                    var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json["rates"].ToString());
                    exchangeRate = rates["TRY"];

                    
                    _cache.Set(cacheKey, exchangeRate, TimeSpan.FromMinutes(10));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Currency API error: {ex.Message}");
                    exchangeRate = 1.0m; 
                }
            }
            return exchangeRate;
        }

        public async Task<ActionResult> Index()
        {
            int TenantId = 1;
            var categories = await _context.Categories
                .Where(c => c.PARENTCATEGORYID == null && !c.ISDELETED && c.TenantId == TenantId)
                .ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Category(string categoryName)
        {
            int TenantId = 1;

            if (string.IsNullOrEmpty(categoryName))
            {
                return NotFound(); 
            }

            string cacheKey = $"Products_Tenant_{TenantId}_Category_{categoryName}";
            if (!_cache.TryGetValue(cacheKey, out List<ProductViewModel> products))
            {
                var selectedCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CATEGORYNAME == categoryName && !c.ISDELETED && c.TenantId == TenantId);

                if (selectedCategory == null)
                {
                    return NotFound();
                }

                var categoryIds = await _context.Categories
                    .Where(c => (c.CATEGORYID == selectedCategory.CATEGORYID || c.PARENTCATEGORYID == selectedCategory.CATEGORYID) && !c.ISDELETED && c.TenantId == TenantId)
                    .Select(c => c.CATEGORYID)
                    .ToListAsync();

                decimal exchangeRate = await GetCurrencyRateAsync();

                products = await _context.Products
                    .Include(p => p.Category)
                    .Include(p => p.ProductProperties)
                        .ThenInclude(pp => pp.Property)
                    .Where(p => categoryIds.Contains(p.CategoryId) && !p.IsDeleted && p.TenantId == TenantId)
                    .Select(p => new ProductViewModel
                    {
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Price = p.Price * exchangeRate,
                        ImagePath = p.ImagePath,
                        CategoryName = p.Category.CATEGORYNAME,
                        Properties = p.ProductProperties
                            .Where(pp => !pp.IsDeleted)
                            .Select(pp => pp.Property.VALUE)
                            .ToList()
                    })
                    .ToListAsync();

                if (products == null || !products.Any())
                {
                    return NotFound();
                }

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };

                _cache.Set(cacheKey, products, cacheOptions);
            }

            ViewBag.CategoryName = categoryName;
            return View("Category", products);
        }
    }
}
