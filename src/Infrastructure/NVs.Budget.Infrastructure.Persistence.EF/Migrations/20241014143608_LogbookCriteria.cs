using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class LogbookCriteria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "StoredTag",
                newName: "Operations_Tags",
                schema: "budget");

            migrationBuilder.AddColumn<string>(
                name: "LogbookCriteria",
                schema: "budget",
                table: "Budgets",
                type: "jsonb",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Operations_Tags",
                schema: "budget");

            migrationBuilder.DropColumn(
                name: "LogbookCriteria",
                schema: "budget",
                table: "Budgets");

            migrationBuilder.CreateTable(
                name: "StoredTag",
                schema: "budget",
                columns: table => new
                {
                    StoredOperationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredTag", x => new { x.StoredOperationId, x.Id });
                    table.ForeignKey(
                        name: "FK_StoredTag_Operations_StoredOperationId",
                        column: x => x.StoredOperationId,
                        principalSchema: "budget",
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}
