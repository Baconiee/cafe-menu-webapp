using CafeMenuWebApp.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CafeMenuWebApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductProperty> ProductProperties { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>().HasData(
                new Category { CATEGORYID = 1, CATEGORYNAME = "Drinks", PARENTCATEGORYID = null, ISDELETED = false, CREATEDDATE = new DateTime(2025, 1, 1), CreatorUserId = 1, ImagePath = "/images/drinks.jpg", TenantId = 1 },
                new Category { CATEGORYID = 2, CATEGORYNAME = "Hot Beverages", PARENTCATEGORYID = 1, ISDELETED = false, CREATEDDATE = new DateTime(2025, 1, 1), CreatorUserId = 1, ImagePath = "/images/hot-beverages.jpg", TenantId = 1 },
                new Category { CATEGORYID = 3, CATEGORYNAME = "Cold Beverages", PARENTCATEGORYID = 1, ISDELETED = false, CREATEDDATE = new DateTime(2025, 1, 1), CreatorUserId = 1, ImagePath = "/images/cold-beverages.jpg", TenantId = 1 },
                new Category { CATEGORYID = 4, CATEGORYNAME = "Desserts", PARENTCATEGORYID = null, ISDELETED = false, CREATEDDATE = new DateTime(2025, 1, 1), CreatorUserId = 1, ImagePath = "/images/desserts.jpg", TenantId = 1 },
                new Category { CATEGORYID = 5, CATEGORYNAME = "Snacks", PARENTCATEGORYID = null, ISDELETED = false, CREATEDDATE = new DateTime(2025, 1, 1), CreatorUserId = 1, ImagePath = "/images/snacks.jpg", TenantId = 1 },
                new Category { CATEGORYID = 6, CATEGORYNAME = "Drinks", PARENTCATEGORYID = null, ISDELETED = false, CREATEDDATE = new DateTime(2025, 3, 1), CreatorUserId = 1, ImagePath = "/images/drinks.jpg", TenantId = 2 }
            );

            modelBuilder.Entity<Product>().HasData(
                new Product { ProductId = 1, ProductName = "Espresso", CategoryId = 2, Price = 2.50m, ImagePath = "/images/espresso.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 2, ProductName = "Cappuccino", CategoryId = 2, Price = 3.00m, ImagePath = "/images/cappuccino.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 3, ProductName = "Iced Coffee", CategoryId = 3, Price = 3.50m, ImagePath = "/images/iced_coffee.jpg", IsDeleted = true, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 4, ProductName = "Lemonade", CategoryId = 3, Price = 2.75m, ImagePath = "/images/lemonade.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 5, ProductName = "Chocolate Cake", CategoryId = 4, Price = 4.00m, ImagePath = "/images/chocolate_cake.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 6, ProductName = "Cheesecake", CategoryId = 4, Price = 4.50m, ImagePath = "/images/cheesecake.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 7, ProductName = "French Fries", CategoryId = 5, Price = 3.00m, ImagePath = "/images/french_fries.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 8, ProductName = "Chicken Nuggets", CategoryId = 5, Price = 4.00m, ImagePath = "/images/chicken_nuggets.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 1, 2), CreatorUserId = 1, TenantId = 1 },
                new Product { ProductId = 9, ProductName = "Tea", CategoryId = 6, Price = 2.00m, ImagePath = "/images/tea.jpg", IsDeleted = false, CreatedDate = new DateTime(2025, 3, 1), CreatorUserId = 1, TenantId = 2 }
            );

            modelBuilder.Entity<Property>().HasData(
                new Property { PROPERTYID = 1, KEY = "Size", VALUE = "Small", TenantId = 1, IsDeleted = false },
                new Property { PROPERTYID = 2, KEY = "Size", VALUE = "Medium", TenantId = 1, IsDeleted = false },
                new Property { PROPERTYID = 3, KEY = "Size", VALUE = "Large", TenantId = 1, IsDeleted = false },
                new Property { PROPERTYID = 4, KEY = "Temperature", VALUE = "Hot", TenantId = 1, IsDeleted = false },
                new Property { PROPERTYID = 5, KEY = "Temperature", VALUE = "Cold", TenantId = 1, IsDeleted = false },
                new Property { PROPERTYID = 6, KEY = "Flavor", VALUE = "Chocolate", TenantId = 1, IsDeleted = false },
                new Property { PROPERTYID = 7, KEY = "Flavor", VALUE = "Vanilla", TenantId = 1, IsDeleted = false },
                new Property { PROPERTYID = 8, KEY = "Flavor", VALUE = "Fresh", TenantId = 1, IsDeleted = false }
            );

            modelBuilder.Entity<ProductProperty>().HasData(
                new ProductProperty { PRODUCTPROPERYID = 1, PRODUCTID = 1, PROPERTYID = 4, TenantId = 1, IsDeleted = false },
                new ProductProperty { PRODUCTPROPERYID = 2, PRODUCTID = 2, PROPERTYID = 4, TenantId = 1, IsDeleted = false },
                new ProductProperty { PRODUCTPROPERYID = 3, PRODUCTID = 3, PROPERTYID = 5, TenantId = 1, IsDeleted = false },
                new ProductProperty { PRODUCTPROPERYID = 4, PRODUCTID = 4, PROPERTYID = 8, TenantId = 1, IsDeleted = false },
                new ProductProperty { PRODUCTPROPERYID = 5, PRODUCTID = 5, PROPERTYID = 6, TenantId = 1, IsDeleted = false },
                new ProductProperty { PRODUCTPROPERYID = 6, PRODUCTID = 6, PROPERTYID = 7, TenantId = 1, IsDeleted = false },
                new ProductProperty { PRODUCTPROPERYID = 7, PRODUCTID = 7, PROPERTYID = 2, TenantId = 1, IsDeleted = false },
                new ProductProperty { PRODUCTPROPERYID = 8, PRODUCTID = 8, PROPERTYID = 2, TenantId = 1, IsDeleted = false }
            );

            byte[] adminHash = new byte[] {
                0xAC, 0x15, 0x9D, 0x34, 0x4B, 0xC6, 0x41, 0xF9,
                0x4C, 0x4F, 0x3E, 0x33, 0x5F, 0x7F, 0x53, 0x5F,
                0xCC, 0x39, 0xC5, 0xC7, 0x6C, 0x14, 0x86, 0xBA,
                0x6E, 0x0B, 0xCB, 0x11, 0xAB, 0xAA, 0x67, 0x90
            };
            byte[] adminSalt = new byte[] {
                0xD3, 0x92, 0xB9, 0x17, 0xC2, 0x84, 0xFB, 0x45,
                0xB7, 0xBB, 0x9C, 0xC3, 0xBF, 0x40, 0xA0, 0x9B
            };

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    USERID = 1,
                    NAME = "Admin",
                    SURNAME = "User",
                    USERNAME = "admin",
                    HASHPASSWORD = adminHash,
                    SALTPASSWORD = adminSalt,
                    TenantId = 1
                }
            );
        }
    }
}