using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeMenuWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedTenantIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                column: "TenantId",
                value: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "Users");
        }
    }
}
