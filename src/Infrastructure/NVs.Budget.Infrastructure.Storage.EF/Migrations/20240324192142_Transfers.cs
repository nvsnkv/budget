using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Storage.Migrations
{
    /// <inheritdoc />
    public partial class Transfers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SinkTransferId",
                table: "Operations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceTransferId",
                table: "Operations",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Transfers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Fee_Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Fee_Currency = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "text", nullable: false),
                    SourceId = table.Column<Guid>(type: "uuid", nullable: false),
                    SinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transfers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transfers_Operations_SinkId",
                        column: x => x.SinkId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Operations_SourceId",
                        column: x => x.SourceId,
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_SinkTransferId",
                table: "Operations",
                column: "SinkTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_SourceTransferId",
                table: "Operations",
                column: "SourceTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SinkId",
                table: "Transfers",
                column: "SinkId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceId",
                table: "Transfers",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Transfers_SinkTransferId",
                table: "Operations",
                column: "SinkTransferId",
                principalTable: "Transfers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Transfers_SourceTransferId",
                table: "Operations",
                column: "SourceTransferId",
                principalTable: "Transfers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Transfers_SinkTransferId",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Transfers_SourceTransferId",
                table: "Operations");

            migrationBuilder.DropTable(
                name: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Operations_SinkTransferId",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_SourceTransferId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "SinkTransferId",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "SourceTransferId",
                table: "Operations");
        }
    }
}
