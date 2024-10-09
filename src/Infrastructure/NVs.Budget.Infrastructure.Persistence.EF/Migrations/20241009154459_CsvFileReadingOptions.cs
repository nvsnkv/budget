using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class CsvFileReadingOptions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CsvFileReadingOptions",
                schema: "budget",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CultureInfo = table.Column<string>(type: "text", nullable: false),
                    DateTimeKind = table.Column<int>(type: "integer", nullable: false),
                    FileNamePattern = table.Column<string>(type: "text", nullable: false),
                    BudgetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvFileReadingOptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CsvFileReadingOptions_Budgets_BudgetId",
                        column: x => x.BudgetId,
                        principalSchema: "budget",
                        principalTable: "Budgets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CsvFileReadingOptions_AttributesConfiguration",
                schema: "budget",
                columns: table => new
                {
                    FileReadingOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Field = table.Column<string>(type: "text", nullable: false),
                    Pattern = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvFileReadingOptions_AttributesConfiguration", x => new { x.FileReadingOptionId, x.Id });
                    table.ForeignKey(
                        name: "FK_CsvFileReadingOptions_AttributesConfiguration_CsvFileReadin~",
                        column: x => x.FileReadingOptionId,
                        principalSchema: "budget",
                        principalTable: "CsvFileReadingOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CsvFileReadingOptions_FieldConfigurations",
                schema: "budget",
                columns: table => new
                {
                    FileReadingOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Field = table.Column<string>(type: "text", nullable: false),
                    Pattern = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CsvFileReadingOptions_FieldConfigurations", x => new { x.FileReadingOptionId, x.Id });
                    table.ForeignKey(
                        name: "FK_CsvFileReadingOptions_FieldConfigurations_CsvFileReadingOpt~",
                        column: x => x.FileReadingOptionId,
                        principalSchema: "budget",
                        principalTable: "CsvFileReadingOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoredValidationRule",
                schema: "budget",
                columns: table => new
                {
                    FileReadingOptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RuleName = table.Column<string>(type: "text", nullable: false),
                    FieldConfiguration = table.Column<string>(type: "text", nullable: false),
                    Condition = table.Column<int>(type: "integer", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredValidationRule", x => new { x.FileReadingOptionId, x.Id });
                    table.ForeignKey(
                        name: "FK_StoredValidationRule_CsvFileReadingOptions_FileReadingOptio~",
                        column: x => x.FileReadingOptionId,
                        principalSchema: "budget",
                        principalTable: "CsvFileReadingOptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CsvFileReadingOptions_BudgetId",
                schema: "budget",
                table: "CsvFileReadingOptions",
                column: "BudgetId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CsvFileReadingOptions_AttributesConfiguration",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "CsvFileReadingOptions_FieldConfigurations",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "StoredValidationRule",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "CsvFileReadingOptions",
                schema: "budget");
        }
    }
}
