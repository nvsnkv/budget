using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class LogbookCriteriaCollection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE budget."Budgets"
                SET "LogbookCriteria" = CASE
                    WHEN "LogbookCriteria" IS NULL THEN '[]'::jsonb
                    WHEN jsonb_typeof("LogbookCriteria") = 'array' THEN "LogbookCriteria"
                    ELSE jsonb_build_array("LogbookCriteria")
                END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE budget."Budgets"
                SET "LogbookCriteria" = CASE
                    WHEN "LogbookCriteria" IS NULL OR "LogbookCriteria" = '[]'::jsonb THEN '{}'::jsonb
                    WHEN jsonb_typeof("LogbookCriteria") = 'array' THEN COALESCE("LogbookCriteria"->0, '{}'::jsonb)
                    ELSE "LogbookCriteria"
                END;
                """);
        }
    }
}
