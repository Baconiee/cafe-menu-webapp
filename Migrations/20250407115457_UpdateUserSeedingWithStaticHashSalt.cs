using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeMenuWebApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserSeedingWithStaticHashSalt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "USERID",
                keyValue: 1,
                columns: new[] { "HASHPASSWORD", "SALTPASSWORD" },
                values: new object[] { new byte[] { 253, 64, 209, 93, 68, 177, 83, 129, 69, 17, 229, 24, 113, 64, 205, 165, 20, 89, 2, 86, 50, 111, 197, 104, 31, 202, 197, 79, 19, 145, 29, 32 }, new byte[] { 219, 18, 144, 114, 204, 127, 112, 174, 98, 193, 7, 138, 133, 213, 102, 123 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "USERID",
                keyValue: 1,
                columns: new[] { "HASHPASSWORD", "SALTPASSWORD" },
                values: new object[] { new byte[] { 159, 50, 69, 156, 66, 168, 30, 210, 129, 94, 227, 201, 114, 250, 209, 96, 63, 228, 60, 50, 26, 53, 36, 243, 6, 151, 116, 11, 187, 253, 207, 195 }, new byte[] { 3, 57, 15, 241, 229, 111, 101, 205, 86, 109, 152, 226, 136, 158, 172, 113 } });
        }
    }
}
