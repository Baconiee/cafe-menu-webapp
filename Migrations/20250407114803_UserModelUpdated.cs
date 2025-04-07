using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeMenuWebApp.Migrations
{
    /// <inheritdoc />
    public partial class UserModelUpdated : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "USERID",
                keyValue: 1,
                columns: new[] { "HASHPASSWORD", "NAME", "SALTPASSWORD", "SURNAME", "USERNAME" },
                values: new object[] { new byte[] { 159, 50, 69, 156, 66, 168, 30, 210, 129, 94, 227, 201, 114, 250, 209, 96, 63, 228, 60, 50, 26, 53, 36, 243, 6, 151, 116, 11, 187, 253, 207, 195 }, "Admin", new byte[] { 3, 57, 15, 241, 229, 111, 101, 205, 86, 109, 152, 226, 136, 158, 172, 113 }, "User", "admin" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Role",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "USERID",
                keyValue: 1,
                columns: new[] { "HASHPASSWORD", "NAME", "Role", "SALTPASSWORD", "SURNAME", "TenantId", "USERNAME" },
                values: new object[] { new byte[] { 143, 67, 67, 70, 100, 143, 107, 150, 223, 137, 221, 169, 1, 197, 23, 107, 16, 166, 216, 57, 97, 221, 60, 26, 200, 139, 89, 178, 220, 50, 122, 164 }, "Berkay", 2, new byte[] { 161, 178, 195, 212, 229, 246, 7, 24, 41, 58, 75, 92, 109, 126, 143, 144 }, "Güler", 1, "berkayadmin" });
        }
    }
}
