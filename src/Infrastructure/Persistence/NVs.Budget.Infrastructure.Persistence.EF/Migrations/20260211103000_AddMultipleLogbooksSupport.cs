using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    [DbContext(typeof(BudgetContext))]
    [Migration("20260211103000_AddMultipleLogbooksSupport")]
    public partial class AddMultipleLogbooksSupport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE budget."Budgets"
                SET "LogbookCriteria" = jsonb_build_array(
                    jsonb_build_object(
                        'CriteriaId', "Id",
                        'Name', 'Default',
                        'Description', COALESCE("LogbookCriteria"->>'Description', ''),
                        'Subcriteria', COALESCE("LogbookCriteria"->'Subcriteria', '[]'::jsonb),
                        'Type', "LogbookCriteria"->'Type',
                        'Tags', COALESCE("LogbookCriteria"->'Tags', '[]'::jsonb),
                        'Substitution', "LogbookCriteria"->'Substitution',
                        'Criteria', "LogbookCriteria"->'Criteria',
                        'IsUniversal', "LogbookCriteria"->'IsUniversal'
                    )
                )
                WHERE "LogbookCriteria" IS NOT NULL;
                """
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE budget."Budgets"
                SET "LogbookCriteria" = CASE
                    WHEN jsonb_typeof("LogbookCriteria") = 'array' AND jsonb_array_length("LogbookCriteria") > 0 THEN jsonb_build_object(
                        'Description', "LogbookCriteria"->0->'Description',
                        'Subcriteria', COALESCE("LogbookCriteria"->0->'Subcriteria', '[]'::jsonb),
                        'Type', "LogbookCriteria"->0->'Type',
                        'Tags', COALESCE("LogbookCriteria"->0->'Tags', '[]'::jsonb),
                        'Substitution', "LogbookCriteria"->0->'Substitution',
                        'Criteria', "LogbookCriteria"->0->'Criteria',
                        'IsUniversal', "LogbookCriteria"->0->'IsUniversal'
                    )
                    ELSE "LogbookCriteria"
                END;
                """
            );
        }
    }
}
