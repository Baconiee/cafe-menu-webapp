using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace CafeMenuWebApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CATEGORYID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CATEGORYNAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PARENTCATEGORYID = table.Column<int>(type: "int", nullable: true),
                    ISDELETED = table.Column<bool>(type: "bit", nullable: false),
                    CREATEDDATE = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CATEGORYID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    USERID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SURNAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    USERNAME = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HASHPASSWORD = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    SALTPASSWORD = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.USERID);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    ProductId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "CATEGORYID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Properties",
                columns: table => new
                {
                    PROPERTYID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KEY = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    VALUE = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Properties", x => x.PROPERTYID);
                    table.ForeignKey(
                        name: "FK_Properties_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "ProductId");
                });

            migrationBuilder.CreateTable(
                name: "ProductProperties",
                columns: table => new
                {
                    PRODUCTPROPERYID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PRODUCTID = table.Column<int>(type: "int", nullable: false),
                    PROPERTYID = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductProperties", x => x.PRODUCTPROPERYID);
                    table.ForeignKey(
                        name: "FK_ProductProperties_Products_PRODUCTID",
                        column: x => x.PRODUCTID,
                        principalTable: "Products",
                        principalColumn: "ProductId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductProperties_Properties_PROPERTYID",
                        column: x => x.PROPERTYID,
                        principalTable: "Properties",
                        principalColumn: "PROPERTYID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "CATEGORYID", "CATEGORYNAME", "CREATEDDATE", "CreatorUserId", "ISDELETED", "ImagePath", "PARENTCATEGORYID", "TenantId" },
                values: new object[,]
                {
                    { 1, "Drinks", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "/images/drinks.jpg", null, 1 },
                    { 2, "Hot Beverages", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "/images/hot-beverages.jpg", 1, 1 },
                    { 3, "Cold Beverages", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "/images/cold-beverages.jpg", 1, 1 },
                    { 4, "Desserts", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "/images/desserts.jpg", null, 1 },
                    { 5, "Snacks", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "/images/snacks.jpg", null, 1 },
                    { 6, "Drinks", new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, false, "/images/drinks.jpg", null, 2 }
                });

            migrationBuilder.InsertData(
                table: "Properties",
                columns: new[] { "PROPERTYID", "IsDeleted", "KEY", "ProductId", "TenantId", "VALUE" },
                values: new object[,]
                {
                    { 1, false, "Size", null, 1, "Small" },
                    { 2, false, "Size", null, 1, "Medium" },
                    { 3, false, "Size", null, 1, "Large" },
                    { 4, false, "Temperature", null, 1, "Hot" },
                    { 5, false, "Temperature", null, 1, "Cold" },
                    { 6, false, "Flavor", null, 1, "Chocolate" },
                    { 7, false, "Flavor", null, 1, "Vanilla" },
                    { 8, false, "Flavor", null, 1, "Fresh" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "USERID", "HASHPASSWORD", "NAME", "SALTPASSWORD", "SURNAME", "TenantId", "USERNAME" },
                values: new object[] { 1, new byte[] { 143, 67, 67, 70, 100, 143, 107, 150, 223, 137, 221, 169, 1, 197, 23, 107, 16, 166, 216, 57, 97, 221, 60, 26, 200, 139, 89, 178, 220, 50, 122, 164 }, "Berkay", new byte[] { 161, 178, 195, 212, 229, 246, 7, 24, 41, 58, 75, 92, 109, 126, 143, 144 }, "Güler", 1, "berkayadmin" });

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "ProductId", "CategoryId", "CreatedDate", "CreatorUserId", "ImagePath", "IsDeleted", "Price", "ProductName", "TenantId" },
                values: new object[,]
                {
                    { 1, 2, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/espresso.jpg", false, 2.50m, "Espresso", 1 },
                    { 2, 2, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/cappuccino.jpg", false, 3.00m, "Cappuccino", 1 },
                    { 3, 3, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/iced_coffee.jpg", true, 3.50m, "Iced Coffee", 1 },
                    { 4, 3, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/lemonade.jpg", false, 2.75m, "Lemonade", 1 },
                    { 5, 4, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/chocolate_cake.jpg", false, 4.00m, "Chocolate Cake", 1 },
                    { 6, 4, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/cheesecake.jpg", false, 4.50m, "Cheesecake", 1 },
                    { 7, 5, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/french_fries.jpg", false, 3.00m, "French Fries", 1 },
                    { 8, 5, new DateTime(2025, 1, 2, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/chicken_nuggets.jpg", false, 4.00m, "Chicken Nuggets", 1 },
                    { 9, 6, new DateTime(2025, 3, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "/images/tea.jpg", false, 2.00m, "Tea", 2 }
                });

            migrationBuilder.InsertData(
                table: "ProductProperties",
                columns: new[] { "PRODUCTPROPERYID", "IsDeleted", "PRODUCTID", "PROPERTYID", "TenantId" },
                values: new object[,]
                {
                    { 1, false, 1, 4, 1 },
                    { 2, false, 2, 4, 1 },
                    { 3, false, 3, 5, 1 },
                    { 4, false, 4, 8, 1 },
                    { 5, false, 5, 6, 1 },
                    { 6, false, 6, 7, 1 },
                    { 7, false, 7, 2, 1 },
                    { 8, false, 8, 2, 1 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProductProperties_PRODUCTID",
                table: "ProductProperties",
                column: "PRODUCTID");

            migrationBuilder.CreateIndex(
                name: "IX_ProductProperties_PROPERTYID",
                table: "ProductProperties",
                column: "PROPERTYID");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Properties_ProductId",
                table: "Properties",
                column: "ProductId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProductProperties");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Properties");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
