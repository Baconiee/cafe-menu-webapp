using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeMenuWebApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminHashSalt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "USERID",
                keyValue: 1,
                columns: new[] { "HASHPASSWORD", "SALTPASSWORD" },
                values: new object[] { new byte[] { 172, 21, 157, 52, 75, 198, 65, 249, 76, 79, 62, 51, 95, 127, 83, 95, 204, 57, 197, 199, 108, 20, 134, 186, 110, 11, 203, 17, 171, 170, 103, 144 }, new byte[] { 211, 146, 185, 23, 194, 132, 251, 69, 183, 187, 156, 195, 191, 64, 160, 155 } });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "USERID",
                keyValue: 1,
                columns: new[] { "HASHPASSWORD", "SALTPASSWORD" },
                values: new object[] { new byte[] { 253, 64, 209, 93, 68, 177, 83, 129, 69, 17, 229, 24, 113, 64, 205, 165, 20, 89, 2, 86, 50, 111, 197, 104, 31, 202, 197, 79, 19, 145, 29, 32 }, new byte[] { 219, 18, 144, 114, 204, 127, 112, 174, 98, 193, 7, 138, 133, 213, 102, 123 } });
        }
    }
}
