using CafeMenuWebApp.Data;
using CafeMenuWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed; 
using System.Text.Json;

namespace CafeMenuWebApp.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IDistributedCache _cache; 
        private readonly IHttpClientFactory _httpClientFactory;

        public CustomerController(ApplicationDbContext context, IDistributedCache cache, IHttpClientFactory httpClientFactory)
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

            string cachedRate = await _cache.GetStringAsync(cacheKey);
            if (cachedRate != null)
            {
                return decimal.Parse(cachedRate); 
            }

            var client = _httpClientFactory.CreateClient();
            string apiKey = "26e9b072bfe042128c6410fb86061fb8";
            string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}&base=USD";

            try
            {
                var response = await client.GetStringAsync(url);
                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
                var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json["rates"].ToString());
                decimal exchangeRate = rates["TRY"];

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                await _cache.SetStringAsync(cacheKey, exchangeRate.ToString(), cacheOptions);

                return exchangeRate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Currency API error: {ex.Message}");
                return 1.0m; 
            }
        }

        public async Task<ActionResult> Index()
        {
            var categories = await _context.Categories
                .Where(c => c.PARENTCATEGORYID == null && !c.ISDELETED && c.TenantId == TenantId)
                .ToListAsync();
            return View(categories);
        }

        public async Task<IActionResult> Category(string categoryName)
        {
            if (string.IsNullOrEmpty(categoryName))
            {
                return NotFound();
            }

            string cacheKey = $"Products_Tenant_{TenantId}_Category_{categoryName}";
            string cachedProducts = await _cache.GetStringAsync(cacheKey);
            List<ProductViewModel> products;

            if (cachedProducts != null)
            {
                products = JsonSerializer.Deserialize<List<ProductViewModel>>(cachedProducts);
                decimal exchangeRate = await GetCurrencyRateAsync();
                foreach (var product in products)
                {
                    product.Price *= exchangeRate; // Fix cached prices
                }
            }
            else
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
                Console.WriteLine($"Exchange rate: {exchangeRate}");

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
                    ViewBag.CategoryName = categoryName;
                    return View("NoProducts");
                }

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), cacheOptions);
            }

            ViewBag.CategoryName = categoryName;
            return View("Category", products);
        }
    }
}