#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class removedBankfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bank",
                schema: "budget",
                table: "Accounts");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bank",
                schema: "budget",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
