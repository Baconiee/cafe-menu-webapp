using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CafeMenuWebApp.Migrations
{
    /// <inheritdoc />
    public partial class AddStoredProcedures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[GeneratePasswordHash]
                    @Username NVARCHAR(50),
                    @Password NVARCHAR(100),
                    @Hash BINARY(32) OUTPUT,
                    @Salt BINARY(16) OUTPUT
                AS
                BEGIN
                    SET @Salt = CRYPT_GEN_RANDOM(16);
                    SET @Hash = HASHBYTES('SHA2_256', CAST(@Password AS NVARCHAR(100)) + CAST(CONVERT(VARCHAR(24), @Salt, 2) AS NVARCHAR(100)));
                END
            ");

            // Optionally create VerifyPassword stored procedure
            migrationBuilder.Sql(@"
                CREATE PROCEDURE [dbo].[VerifyPassword]
                    @Username NVARCHAR(50),
                    @Password NVARCHAR(100),
                    @IsValid BIT OUTPUT
                AS
                BEGIN
                    DECLARE @StoredHash BINARY(32), @StoredSalt BINARY(16);

                    SELECT @StoredHash = HASHPASSWORD, @StoredSalt = SALTPASSWORD
                    FROM Users
                    WHERE USERNAME = @Username;

                    IF @StoredHash IS NULL
                    BEGIN
                        SET @IsValid = 0;
                        RETURN;
                    END

                    DECLARE @ComputedHash BINARY(32);
                    SET @ComputedHash = HASHBYTES('SHA2_256', CAST(@Password AS NVARCHAR(100)) + CAST(CONVERT(VARCHAR(24), @StoredSalt, 2) AS NVARCHAR(100)));

                    SET @IsValid = CASE WHEN @ComputedHash = @StoredHash THEN 1 ELSE 0 END;
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[GeneratePasswordHash]");
            migrationBuilder.Sql("DROP PROCEDURE [dbo].[VerifyPassword]");
        }
    }
}
