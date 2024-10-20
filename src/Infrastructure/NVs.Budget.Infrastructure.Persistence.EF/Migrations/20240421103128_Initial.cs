#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "budget");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sle,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,ved,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwl");

            migrationBuilder.CreateTable(
                name: "Accounts",
                schema: "budget",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Bank = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Owners",
                schema: "budget",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Owners", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rates",
                schema: "budget",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AsOf = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    From = table.Column<int>(type: "integer", nullable: false),
                    To = table.Column<int>(type: "integer", nullable: false),
                    Rate = table.Column<decimal>(type: "numeric", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rates_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalSchema: "budget",
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StoredAccountStoredOwner",
                schema: "budget",
                columns: table => new
                {
                    AccountsId = table.Column<Guid>(type: "uuid", nullable: false),
                    OwnersId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StoredAccountStoredOwner", x => new { x.AccountsId, x.OwnersId });
                    table.ForeignKey(
                        name: "FK_StoredAccountStoredOwner_Accounts_AccountsId",
                        column: x => x.AccountsId,
                        principalSchema: "budget",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StoredAccountStoredOwner_Owners_OwnersId",
                        column: x => x.OwnersId,
                        principalSchema: "budget",
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Operations",
                schema: "budget",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Amount_Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Amount_Currency = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<string>(type: "text", nullable: true),
                    Attributes = table.Column<string>(type: "jsonb", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceTransferId = table.Column<Guid>(type: "uuid", nullable: true),
                    SinkTransferId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Operations_Accounts_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "budget",
                        principalTable: "Accounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "Transfers",
                schema: "budget",
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
                        principalSchema: "budget",
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transfers_Operations_SourceId",
                        column: x => x.SourceId,
                        principalSchema: "budget",
                        principalTable: "Operations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Operations_AccountId",
                schema: "budget",
                table: "Operations",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_SinkTransferId",
                schema: "budget",
                table: "Operations",
                column: "SinkTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Operations_SourceTransferId",
                schema: "budget",
                table: "Operations",
                column: "SourceTransferId");

            migrationBuilder.CreateIndex(
                name: "IX_Owners_UserId",
                schema: "budget",
                table: "Owners",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Rates_OwnerId",
                schema: "budget",
                table: "Rates",
                column: "OwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_StoredAccountStoredOwner_OwnersId",
                schema: "budget",
                table: "StoredAccountStoredOwner",
                column: "OwnersId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SinkId",
                schema: "budget",
                table: "Transfers",
                column: "SinkId");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceId",
                schema: "budget",
                table: "Transfers",
                column: "SourceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Transfers_SinkTransferId",
                schema: "budget",
                table: "Operations",
                column: "SinkTransferId",
                principalSchema: "budget",
                principalTable: "Transfers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Operations_Transfers_SourceTransferId",
                schema: "budget",
                table: "Operations",
                column: "SourceTransferId",
                principalSchema: "budget",
                principalTable: "Transfers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Accounts_AccountId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Transfers_SinkTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Transfers_SourceTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropTable(
                name: "Rates",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "StoredAccountStoredOwner",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "StoredTag",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "Owners",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "Accounts",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "Transfers",
                schema: "budget");

            migrationBuilder.DropTable(
                name: "Operations",
                schema: "budget");
        }
    }
}
