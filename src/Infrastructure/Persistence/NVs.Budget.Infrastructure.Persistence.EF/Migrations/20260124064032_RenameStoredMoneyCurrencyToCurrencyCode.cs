using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class RenameStoredMoneyCurrencyToCurrencyCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fee_Currency",
                schema: "budget",
                table: "Transfers",
                newName: "Fee_CurrencyCode");

            migrationBuilder.RenameColumn(
                name: "Amount_Currency",
                schema: "budget",
                table: "Operations",
                newName: "Amount_CurrencyCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Fee_CurrencyCode",
                schema: "budget",
                table: "Transfers",
                newName: "Fee_Currency");

            migrationBuilder.RenameColumn(
                name: "Amount_CurrencyCode",
                schema: "budget",
                table: "Operations",
                newName: "Amount_Currency");
        }
    }
}
