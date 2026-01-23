using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class AddOperationNotes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Notes",
                schema: "budget",
                table: "Operations",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Notes",
                schema: "budget",
                table: "Operations");
        }
    }
}
