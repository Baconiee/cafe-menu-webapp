using CafeMenuWebApp.Data;
using CafeMenuWebApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using CafeMenuWebApp.Views.Admin;

namespace CafeMenuWebApp.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        public readonly IDistributedCache _cache;
        public readonly IHttpClientFactory _httpClientFactory;


        public AdminController(ApplicationDbContext context, IDistributedCache cache, IHttpClientFactory httpClientFactory)
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

        public async Task<IActionResult> Categories()
        {
            
            var categories = await _context.Categories
                .Where(c => c.TenantId == TenantId && !c.ISDELETED)
                .Select(c => new CategoryViewModel
                {
                    CategoryId = c.CATEGORYID,
                    CategoryName = c.CATEGORYNAME,
                    ParentCategoryId = c.PARENTCATEGORYID,
                    ParentCategoryName = c.PARENTCATEGORYID.HasValue
                        ? _context.Categories
                            .Where(pc => pc.CATEGORYID == c.PARENTCATEGORYID && !pc.ISDELETED)
                            .Select(pc => pc.CATEGORYNAME)
                            .FirstOrDefault()
                        : null,
                    TenantId = c.TenantId,
                })
                .ToListAsync();

            return View(categories);
        }

        public IActionResult AddCategory()
        {
            
            ViewBag.ParentCategories = new SelectList(
                _context.Categories
                    .Where(c => c.TenantId == TenantId && !c.ISDELETED)
                    .Select(c => new { c.CATEGORYID, c.CATEGORYNAME }),
                "CATEGORYID",
                "CATEGORYNAME"
            );
            return View(new Category());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(Category category, IFormFile imageFile)
        {
            

            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    category.ImagePath = $"/images/{fileName}";
                }

                category.TenantId = TenantId;
                category.ISDELETED = false;
                category.CREATEDDATE = DateTime.Now;
                category.CreatorUserId = 1;

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                var cacheKeyTopLevel = $"DashboardData_Tenant_{TenantId}";
                var cacheKeyCategory = $"Products_Tenant_{TenantId}_Category_{category.CATEGORYNAME}";

                await _cache.RemoveAsync(cacheKeyTopLevel);
                await _cache.RemoveAsync(cacheKeyCategory);

                return RedirectToAction("Categories");
            }

            ViewBag.ParentCategories = new SelectList(
                _context.Categories
                    .Where(c => c.TenantId == TenantId && !c.ISDELETED)
                    .Select(c => new { c.CATEGORYID, c.CATEGORYNAME }),
                "CATEGORYID",
                "CATEGORYNAME",
                category.PARENTCATEGORYID
            );
            return View(category);
        }

        public async Task<IActionResult> EditCategory(int id)
        {
            
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CATEGORYID == id && c.TenantId == TenantId && !c.ISDELETED);

            if (category == null)
            {
                return NotFound();
            }

            var model = new EditCategoryViewModel
            {
                CATEGORYID = category.CATEGORYID,
                CATEGORYNAME = category.CATEGORYNAME,
                PARENTCATEGORYID = category.PARENTCATEGORYID,
                CurrentImagePath = category.ImagePath
            };

            ViewBag.ParentCategories = new SelectList(
                _context.Categories
                    .Where(c => c.TenantId == TenantId && c.CATEGORYID != id && !c.ISDELETED)
                    .Select(c => new { c.CATEGORYID, c.CATEGORYNAME }),
                "CATEGORYID",
                "CATEGORYNAME",
                category.PARENTCATEGORYID
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(EditCategoryViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CATEGORYID == model.CATEGORYID && c.TenantId == TenantId && !c.ISDELETED);

                if (existingCategory == null)
                {
                    return NotFound();
                }

                existingCategory.CATEGORYNAME = model.CATEGORYNAME;
                existingCategory.PARENTCATEGORYID = model.PARENTCATEGORYID;

                if (model.ImageFile != null && model.ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(model.ImageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    }
                    existingCategory.ImagePath = $"/images/{fileName}";
                }

                _context.Categories.Update(existingCategory);
                await _context.SaveChangesAsync();

                var cacheKey = $"DashboardData_Tenant_{TenantId}";
                await _cache.RemoveAsync(cacheKey);

                return RedirectToAction("Categories");
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine($"Validation Error: {error.ErrorMessage}");
            }

            ViewBag.ParentCategories = new SelectList(
                _context.Categories
                    .Where(c => c.TenantId == TenantId && !c.ISDELETED && c.CATEGORYID != model.CATEGORYID)
                    .Select(c => new { c.CATEGORYID, c.CATEGORYNAME }),
                "CATEGORYID",
                "CATEGORYNAME",
                model.PARENTCATEGORYID
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CATEGORYID == id && c.TenantId == TenantId && !c.ISDELETED);

            var productIds = await _context.Products
                .Where(p => p.CategoryId == id && p.TenantId == TenantId && !p.IsDeleted)
                .Select(p => p.ProductId)
                .ToListAsync();

            var productProperties = await _context.ProductProperties
                .Where(pp => productIds.Contains(pp.PRODUCTID) && pp.TenantId == TenantId && !pp.IsDeleted)
                .ToListAsync();

            if (productProperties != null && productProperties.Any())
            {
                foreach (var productProperty in productProperties)
                {
                    productProperty.IsDeleted = true;
                }
            }

            _context.ProductProperties.UpdateRange(productProperties);

            if (category == null)
            {
                return NotFound();
            }

            category.ISDELETED = true;
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();

            var cacheKey = $"DashboardData_Tenant_{TenantId}";
            await _cache.RemoveAsync(cacheKey);


            return RedirectToAction("Categories");
        }

        public async Task<IActionResult> Products(int categoryId, int page = 1)
        {
            
            int pageSize = 8;

            var productsQuery = _context.Products
                .Include(p => p.ProductProperties)
                    .ThenInclude(pp => pp.Property)
                .Where(p => p.CategoryId == categoryId && !p.IsDeleted && p.TenantId == TenantId)
                .OrderBy(p => p.ProductName);

            var totalProducts = await productsQuery.CountAsync();

            var products = await productsQuery
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    ProductName = p.ProductName,
                    Price = p.Price,
                    ImagePath = p.ImagePath,
                    CategoryId = p.CategoryId,
                    Properties = p.ProductProperties
                        .Where(pp => !pp.IsDeleted)
                        .Select(pp => pp.Property.VALUE).ToList()
                })
                .ToListAsync();

            ViewBag.CategoryName = await _context.Categories
                .Where(c => c.CATEGORYID == categoryId)
                .Select(c => c.CATEGORYNAME)
                .FirstOrDefaultAsync();
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalProducts / (double)pageSize);
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        public async Task<IActionResult> AddProduct(int categoryId)
        {
            
            var product = new Product
            {
                CategoryId = categoryId
            };

            ViewBag.CategoryName = await _context.Categories
                .Where(c => c.CATEGORYID == categoryId)
                .Select(c => c.CATEGORYNAME)
                .FirstOrDefaultAsync();

            ViewBag.Properties = new SelectList(
                await _context.Properties
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.PROPERTYID, Display = $"{p.KEY}: {p.VALUE}" })
                    .ToListAsync(),
                "PROPERTYID",
                "Display"
            );

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile imageFile)
        {
            

            if (ModelState.IsValid)
            {
                Console.WriteLine("ModelState is valid");

                try
                {
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        product.ImagePath = $"/images/{fileName}";
                        Console.WriteLine($"Image saved: {product.ImagePath}");
                    }

                    product.TenantId = TenantId;
                    product.IsDeleted = false;
                    product.CreatorUserId = 1;

                    if (product.SelectedPropertyIds != null && product.SelectedPropertyIds.Length > 0)
                    {
                        product.ProductProperties = product.SelectedPropertyIds.Select(id => new ProductProperty
                        {
                            PRODUCTID = product.ProductId,
                            PROPERTYID = id,
                            TenantId = TenantId
                        }).ToList();
                    }

                    _context.Products.Add(product);
                    await _context.SaveChangesAsync();

                    var cacheKey = $"DashboardData_Tenant_{TenantId}";
                    await _cache.RemoveAsync(cacheKey);

                    Console.WriteLine($"Product saved with ID: {product.ProductId}");

                    if (product.ProductProperties.Any())
                    {
                        foreach (var pp in product.ProductProperties)
                        {
                            pp.PRODUCTID = product.ProductId;
                        }
                        _context.ProductProperties.UpdateRange(product.ProductProperties);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("ProductProperties updated");
                    }

                    return RedirectToAction("Products", new { categoryId = product.CategoryId });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    ModelState.AddModelError("", "An error occurred while saving the product.");
                }
            }

            Console.WriteLine("ModelState is invalid or error occurred");
            var errors = ModelState.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
            foreach (var error in errors)
            {
                Console.WriteLine($"HATA: {error}");
            }

            ViewBag.CategoryName = await _context.Categories
                .Where(c => c.CATEGORYID == product.CategoryId)
                .Select(c => c.CATEGORYNAME)
                .FirstOrDefaultAsync();

            ViewBag.Properties = new SelectList(
                await _context.Properties
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.PROPERTYID, Display = $"{p.KEY}: {p.VALUE}" })
                    .ToListAsync(),
                "PROPERTYID",
                "Display",
                product.SelectedPropertyIds
            );

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id) 
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TenantId == TenantId && !p.IsDeleted);
            if (product == null)
            {
                return NotFound();
            }

            product.IsDeleted = true;

            var associatedProductProperties = await _context.ProductProperties
                .Where(pp => pp.PRODUCTID == id && pp.TenantId == TenantId && !pp.IsDeleted)
                .ToListAsync();

            if (associatedProductProperties != null)
            {
                foreach (var productProperty in associatedProductProperties)
                {
                    productProperty.IsDeleted = true;
                }

                _context.ProductProperties.UpdateRange(associatedProductProperties);
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            var categoryName = product.Category.CATEGORYNAME;

            if (!string.IsNullOrEmpty(categoryName))
            {
                string newCacheKey = $"Products_Tenant_{TenantId}_Category_{categoryName}";
                await _cache.RemoveAsync(newCacheKey); 
            }

            var cacheKey = $"DashboardData_Tenant_{TenantId}";
            await _cache.RemoveAsync(cacheKey); 

            return RedirectToAction("Products", new { categoryId = product.CategoryId });
        }

        public async Task<IActionResult> EditProduct(int id)
        {
            

            var product = await _context.Products
                .Include(p => p.ProductProperties)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.TenantId == TenantId && !p.IsDeleted);

            if (product == null)
            {
                return NotFound();
            }

            product.SelectedPropertyIds = product.ProductProperties
                .Select(pp => pp.PROPERTYID)
                .ToArray();

            ViewBag.Properties = new SelectList(
                await _context.Properties
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.PROPERTYID, Display = $"{p.KEY}: {p.VALUE}" })
                    .ToListAsync(),
                "PROPERTYID",
                "Display",
                product.SelectedPropertyIds
            );

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product model, IFormFile imageFile)
        {
            

            if (ModelState.ContainsKey("imageFile"))
            {
                ModelState.Remove("imageFile");
            }

            if (ModelState.IsValid)
            {
                var product = await _context.Products
                    .Include(p => p.ProductProperties)
                    .FirstOrDefaultAsync(p => p.ProductId == model.ProductId && p.TenantId == TenantId && !p.IsDeleted);

                if (product == null)
                {
                    return NotFound();
                }

                product.ProductName = model.ProductName;
                product.Price = model.Price;

                product.ProductProperties.Clear();
                if (model.SelectedPropertyIds != null && model.SelectedPropertyIds.Any())
                {
                    product.ProductProperties = model.SelectedPropertyIds
                        .Select(p => new ProductProperty
                        {
                            PRODUCTID = model.ProductId,
                            PROPERTYID = p,
                            TenantId = TenantId,
                            IsDeleted = false
                        })
                        .ToList();
                }

                if (imageFile != null && imageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(imageFile.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.ImagePath = $"/images/{fileName}";
                }

                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction("Products", new { categoryId = product.CategoryId });
            }

            var errors = ModelState
                .Where(m => m.Value.Errors.Count > 0)
                .Select(m => $"{m.Key}: {string.Join(", ", m.Value.Errors.Select(e => e.ErrorMessage))}");
            foreach (var error in errors)
            {
                Console.WriteLine($"Validation Error: {error}");
            }

            ViewBag.Properties = new SelectList(
                await _context.Properties
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.PROPERTYID, Display = $"{p.KEY}: {p.VALUE}" })
                    .ToListAsync(),
                "PROPERTYID",
                "Display",
                model.SelectedPropertyIds
            );

            return View(model);
        }

        public async Task<IActionResult> Properties()
        {
            
            var properties = await _context.Properties
                .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                .ToListAsync();

            return View(properties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProperty(int propertyId)
        {
            
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PROPERTYID == propertyId && !p.IsDeleted);

            if (property == null)
            {
                return NotFound();
            }

            property.IsDeleted = true;

            var productProperties = await _context.ProductProperties
                .Where(pp => pp.PROPERTYID == propertyId && !pp.IsDeleted)
                .ToListAsync();

            foreach (var pp in productProperties)
            {
                pp.IsDeleted = true;
            }

            var productId = await _context.ProductProperties
                .Where(pp => pp.PROPERTYID == propertyId && pp.TenantId == TenantId && !pp.IsDeleted)
                .Select(p => p.PRODUCTID)
                .FirstOrDefaultAsync();

            var categoryName = await _context.Products
                .Where(p => p.ProductId == productId && p.TenantId == TenantId && !p.IsDeleted)
                .Select(p => p.Category.CATEGORYNAME)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(categoryName))
            {
                string newCacheKey = $"Products_Tenant_{TenantId}_Category_{categoryName}";
                await _cache.RemoveAsync(newCacheKey);
            }


            _context.Properties.Update(property);
            _context.ProductProperties.UpdateRange(productProperties);

            await _context.SaveChangesAsync();
            return RedirectToAction("Properties");
        }

        public async Task<IActionResult> EditProperty(int propertyId)
        {
            
            var property = await _context.Properties
                .FirstOrDefaultAsync(p => p.PROPERTYID == propertyId && p.TenantId == TenantId && !p.IsDeleted);

            if (property == null)
            {
                return NotFound();
            }

            return View(property);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProperty(CafeMenuWebApp.Models.Property model)
        {
            

            if (ModelState.IsValid)
            {
                var property = await _context.Properties
                    .FirstOrDefaultAsync(p => p.PROPERTYID == model.PROPERTYID && p.TenantId == TenantId && !p.IsDeleted);

                if (property == null)
                {
                    return NotFound();
                }

                property.VALUE = model.VALUE;

                _context.Properties.Update(property);
                await _context.SaveChangesAsync();

                var affectedProductIds = await _context.ProductProperties
                    .Where(pp => pp.PROPERTYID == model.PROPERTYID && pp.TenantId == TenantId && !pp.IsDeleted)
                    .Select(pp => pp.PRODUCTID)
                    .ToListAsync();

                var affectedCategoryNames = await _context.Products
                    .Where(p => affectedProductIds.Contains(p.ProductId) && !p.IsDeleted)
                    .Select(p => p.Category.CATEGORYNAME)
                    .Distinct()
                    .ToListAsync();

                foreach (var categoryName in affectedCategoryNames)
                {
                    string cacheKey = $"{categoryName}Products";
                    await _cache.RemoveAsync(cacheKey);
                }

                return RedirectToAction("Properties");
            }

            return View("EditProperty", model);
        }

        public async Task<IActionResult> AddProperty()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProperty(CafeMenuWebApp.Models.Property model)
        {
            
            if (ModelState.IsValid)
            {
                model.TenantId = TenantId;
                model.IsDeleted = false;

                _context.Properties.Add(model);
                await _context.SaveChangesAsync();

                return RedirectToAction("Properties");
            }
            
            return View("AddProperty", model);
        }

        public async Task<IActionResult> ProductProperties()
        {
            
            var productProperties = await _context.ProductProperties
                .Include(pp => pp.Product)
                .Include(pp => pp.Property)
                .Where(pp => pp.TenantId == TenantId && !pp.IsDeleted)
                .ToListAsync();

            ViewBag.Products = new SelectList(
                await _context.Products
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.ProductId, p.ProductName })
                    .ToListAsync(),
                "ProductId",
                "ProductName"
            );

            ViewBag.Properties = new SelectList(
                await _context.Properties
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.PROPERTYID, Display = $"{p.KEY}: {p.VALUE}" })
                    .ToListAsync(),
                "PROPERTYID",
                "Display"
            );

            return View(productProperties);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductProperty(int PRODUCTID, int[] PROPERTYIDs)
        {
            
            var existingProductProperties = await _context.ProductProperties
                .Where(pp => pp.PRODUCTID == PRODUCTID && pp.TenantId == TenantId && !pp.IsDeleted)
                .Select(pp => pp.PROPERTYID)
                .ToListAsync();

            var newProductProperties = new List<ProductProperty>();

            foreach (var propertyId in PROPERTYIDs)
            {
                if (!existingProductProperties.Contains(propertyId))
                {
                    newProductProperties.Add(new ProductProperty
                    {
                        PRODUCTID = PRODUCTID,
                        PROPERTYID = propertyId,
                        TenantId = TenantId,
                        IsDeleted = false
                    });
                }
            }

            if (newProductProperties.Any())
            {
                _context.ProductProperties.AddRange(newProductProperties);
                await _context.SaveChangesAsync();

                var categoryName = await _context.Products
                    .Where(p => p.ProductId == PRODUCTID && !p.IsDeleted)
                    .Select(p => p.Category.CATEGORYNAME)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(categoryName))
                {
                    string cacheKey = $"Products_Tenant_{TenantId}_Category_{categoryName}";
                    await _cache.RemoveAsync(cacheKey);
                }
            }
            else
            {
                TempData["Error"] = "All selected properties are already assigned to this product.";
            }

            return RedirectToAction("ProductProperties");
        }

        public async Task<IActionResult> EditProductProperty(int id)
        {
            
            var productProperty = await _context.ProductProperties
                .Include(pp => pp.Product)
                .Include(pp => pp.Property)
                .FirstOrDefaultAsync(pp => pp.PRODUCTPROPERYID == id && pp.TenantId == TenantId && !pp.IsDeleted);

            if (productProperty == null)
            {
                return NotFound();
            }

            ViewBag.Products = new SelectList
            (
                await _context.Products
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.ProductId, p.ProductName })
                    .ToListAsync(),
                "ProductId",
                "ProductName",
                productProperty.PRODUCTID
            );

            ViewBag.Properties = new SelectList
            (
                await _context.Properties
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.PROPERTYID, p.VALUE })
                    .ToListAsync(),
                "PROPERTYID",
                "VALUE",
                productProperty.PROPERTYID
            );

            return View(productProperty);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProductProperty(ProductProperty model)
        {
            

            if (ModelState.IsValid)
            {
                var productProperty = await _context.ProductProperties
                    .FirstOrDefaultAsync(pp => pp.PRODUCTPROPERYID == model.PRODUCTPROPERYID && pp.TenantId == TenantId && !pp.IsDeleted);

                if (productProperty == null)
                {
                    return NotFound();
                }

                int oldProductId = productProperty.PRODUCTID;

                productProperty.PRODUCTID = model.PRODUCTID;
                productProperty.PROPERTYID = model.PROPERTYID;

                _context.ProductProperties.Update(productProperty);
                await _context.SaveChangesAsync();

                var oldCategoryName = await _context.Products
                    .Where(p => p.ProductId == oldProductId && p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => p.Category.CATEGORYNAME)
                    .FirstOrDefaultAsync();

                var newCategoryName = await _context.Products
                    .Where(p => p.ProductId == productProperty.PRODUCTID && p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => p.Category.CATEGORYNAME)
                    .FirstOrDefaultAsync();

                if (!string.IsNullOrEmpty(oldCategoryName))
                {
                    string oldCacheKey = $"Products_Tenant_{TenantId}_Category_{oldCategoryName}";
                    await _cache.RemoveAsync(oldCacheKey);
                }
                if (!string.IsNullOrEmpty(newCategoryName) && newCategoryName != oldCategoryName)
                {
                    string newCacheKey = $"Products_Tenant_{TenantId}_Category_{newCategoryName}";
                    await _cache.RemoveAsync(newCacheKey);
                }

                return RedirectToAction("ProductProperties");
            }

            var errors = ModelState.Select(kvp => $"{kvp.Key}: {string.Join(", ", kvp.Value.Errors.Select(e => e.ErrorMessage))}");
            foreach (var error in errors)
            {
                Console.WriteLine($"HATA: {error}");
            }

            ViewBag.Products = new SelectList(
                await _context.Products
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.ProductId, p.ProductName })
                    .ToListAsync(),
                "ProductId",
                "ProductName",
                model.PRODUCTID
            );
            ViewBag.Properties = new SelectList(
                await _context.Properties
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted)
                    .Select(p => new { p.PROPERTYID, p.VALUE })
                    .ToListAsync(),
                "PROPERTYID",
                "VALUE",
                model.PROPERTYID
            );

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductProperty(int id)
        {
            
            var productProperty = await _context.ProductProperties
                .FirstOrDefaultAsync(pp => pp.PRODUCTPROPERYID == id && pp.TenantId == TenantId && !pp.IsDeleted);

            if (productProperty == null)
            {
                return NotFound();
            }

            productProperty.IsDeleted = true;
            await _context.SaveChangesAsync();

            var categoryName = await _context.Products
                .Where(p => p.ProductId == productProperty.PRODUCTID && p.TenantId == TenantId && !p.IsDeleted)
                .Select(p => p.Category.CATEGORYNAME)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(categoryName))
            {
                string newCacheKey = $"Products_Tenant_{TenantId}_Category_{categoryName}";
                await _cache.RemoveAsync(newCacheKey);
            }

            return RedirectToAction("ProductProperties");
        }

        public async Task<IActionResult> Index()
        {
            var cacheKey = $"DashboardData_Tenant_{TenantId}";
            string cachedData = await _cache.GetStringAsync(cacheKey);
            List<ProductCountByCategoryViewModel> productCounts;
            decimal currencyRate;

            if (cachedData != null)
            {
                var cachedDashboard = JsonSerializer.Deserialize<DashboardViewModel>(cachedData);
                productCounts = cachedDashboard.ProductCounts;
                currencyRate = cachedDashboard.CurrencyRate;
            }
            else
            {
                var categories = await _context.Categories
                    .Where(c => c.TenantId == TenantId && !c.ISDELETED)
                    .ToListAsync();

                var products = await _context.Products
                    .Where(p => p.TenantId == TenantId && !p.IsDeleted && p.Category != null && !p.Category.ISDELETED)
                    .Select(p => new { p.CategoryId, p.Category.CATEGORYNAME })
                    .ToListAsync();

                var topLevelCounts = new Dictionary<int, ProductCountByCategoryViewModel>();
                foreach (var product in products)
                {
                    int topLevelId = await GetTopLevelCategoryIdAsync(product.CategoryId);
                    var topLevelCategory = categories.FirstOrDefault(c => c.CATEGORYID == topLevelId);
                    if (topLevelCategory != null)
                    {
                        if (!topLevelCounts.TryGetValue(topLevelId, out var count))
                        {
                            count = new ProductCountByCategoryViewModel { CategoryName = topLevelCategory.CATEGORYNAME };
                            topLevelCounts[topLevelId] = count;
                        }
                        count.ProductCount++;
                    }
                }

                foreach (var topLevel in topLevelCounts.Values)
                {
                    var subCats = categories
                        .Where(c => c.PARENTCATEGORYID == categories.First(cat => cat.CATEGORYNAME == topLevel.CategoryName).CATEGORYID)
                        .Select(c => new ProductCountByCategoryViewModel
                        {
                            CategoryName = c.CATEGORYNAME,
                            ProductCount = products.Count(p => p.CategoryId == c.CATEGORYID)
                        })
                        .Where(sc => sc.ProductCount > 0)
                        .ToList();

                    topLevel.SubCategories = subCats;
                    topLevel.HasSubCategories = subCats.Any();
                }

                productCounts = topLevelCounts.Values.ToList();
                currencyRate = await GetCurrencyRateAsync(); 

                var dashboardData = new DashboardViewModel
                {
                    ProductCounts = productCounts,
                    CurrencyRate = currencyRate
                };
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) 
                };
                await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(dashboardData), cacheOptions);
            }

            ViewBag.ProductCounts = productCounts;
            ViewBag.CurrencyRate = currencyRate;

            return View();
        }

        private async Task<int> GetTopLevelCategoryIdAsync(int categoryId)
        {
            
            var category = await _context.Categories
                .FirstOrDefaultAsync(c => c.CATEGORYID == categoryId && !c.ISDELETED && c.TenantId == TenantId);

            if (category == null || category.PARENTCATEGORYID == null)
                return categoryId;

            return await GetTopLevelCategoryIdAsync(category.PARENTCATEGORYID.Value);
        }

        private async Task<decimal> GetCurrencyRateAsync()
        {
            const string cacheKey = "USdtoTRYRate";
            string cachedRate = await _cache.GetStringAsync(cacheKey);
            if (!string.IsNullOrEmpty(cachedRate))
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Cache HIT: Rate = {cachedRate}");
                return decimal.Parse(cachedRate);
            }

            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Cache MISS: Fetching from API...");
            var client = _httpClientFactory.CreateClient();
            string apiKey = "26e9b072bfe042128c6410fb86061fb8";
            string url = $"https://openexchangerates.org/api/latest.json?app_id={apiKey}&base=USD";

            try
            {
                var response = await client.GetStringAsync(url);
                var json = JsonSerializer.Deserialize<Dictionary<string, object>>(response);
                var rates = JsonSerializer.Deserialize<Dictionary<string, decimal>>(json["rates"].ToString());
                decimal rate = rates["TRY"];

                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(10)
                };
                await _cache.SetStringAsync(cacheKey, rate.ToString(), cacheOptions);
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Cached new rate: {rate}");

                return rate;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] Currency API error: {ex.Message}");
                return 1.0m;
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCurrencyRate()
        {
            var rate = await GetCurrencyRateAsync(); 
            return Content(rate.ToString());
        }
    }
    }
