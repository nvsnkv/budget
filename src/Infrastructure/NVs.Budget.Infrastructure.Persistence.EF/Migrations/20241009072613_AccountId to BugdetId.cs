using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class AccountIdtoBugdetId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Accounts_AccountId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.RenameColumn(
                name: "AccountId",
                schema: "budget",
                table: "Operations",
                newName: "BudgetId");

            migrationBuilder.RenameIndex(
                name: "IX_Operations_AccountId",
                schema: "budget",
                table: "Operations",
                newName: "IX_Operations_BudgetId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Accounts_BudgetId",
                schema: "budget",
                table: "Operations",
                column: "BudgetId",
                principalSchema: "budget",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Accounts_BudgetId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.RenameColumn(
                name: "BudgetId",
                schema: "budget",
                table: "Operations",
                newName: "AccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Operations_BudgetId",
                schema: "budget",
                table: "Operations",
                newName: "IX_Operations_AccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Accounts_AccountId",
                schema: "budget",
                table: "Operations",
                column: "AccountId",
                principalSchema: "budget",
                principalTable: "Accounts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
