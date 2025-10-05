using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class RenameAccountToBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            /* odd. Generated but failed to apply
            TODO investigate `42704: constraint "FK_StoredBudgetStoredOwner_Budgets_AccountsId" of relation "StoredBudgetStoredOwner" does not exist` error
            migrationBuilder.DropForeignKey(
                name: "FK_StoredBudgetStoredOwner_Budgets_AccountsId",
                schema: "budget",
                table: "StoredBudgetStoredOwner");
            */
            
            migrationBuilder.RenameColumn(
                name: "AccountsId",
                schema: "budget",
                table: "StoredBudgetStoredOwner",
                newName: "BudgetsId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredBudgetStoredOwner_Budgets_BudgetsId",
                schema: "budget",
                table: "StoredBudgetStoredOwner",
                column: "BudgetsId",
                principalSchema: "budget",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_StoredBudgetStoredOwner_Budgets_BudgetsId",
                schema: "budget",
                table: "StoredBudgetStoredOwner");

            migrationBuilder.RenameColumn(
                name: "BudgetsId",
                schema: "budget",
                table: "StoredBudgetStoredOwner",
                newName: "AccountsId");

            migrationBuilder.AddForeignKey(
                name: "FK_StoredBudgetStoredOwner_Budgets_AccountsId",
                schema: "budget",
                table: "StoredBudgetStoredOwner",
                column: "AccountsId",
                principalSchema: "budget",
                principalTable: "Budgets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
