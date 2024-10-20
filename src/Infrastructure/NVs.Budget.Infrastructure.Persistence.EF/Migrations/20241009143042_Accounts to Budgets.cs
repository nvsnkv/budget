using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class AccountstoBudgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Accounts_BudgetId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.RenameTable(
                name: "StoredAccountStoredOwner",
                newName: "StoredBudgetStoredOwner",
                schema: "budget");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Budgets",
                schema: "budget");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Budgets_BudgetId",
                schema: "budget",
                table: "Operations",
                column: "BudgetId",
                principalSchema: "budget",
                principalTable: "Budgets",
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

            migrationBuilder.RenameTable(
                name: "StoredBudgetStoredOwner",
                newName: "StoredAccountStoredOwner",
                schema: "budget");

            migrationBuilder.RenameTable(
                name: "Budgets",
                newName: "Accounts",
                schema: "budget");

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
    }
}
