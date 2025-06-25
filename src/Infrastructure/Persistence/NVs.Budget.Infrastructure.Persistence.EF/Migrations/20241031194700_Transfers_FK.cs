using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class Transfers_FK : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Transfers_SinkTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropForeignKey(
                name: "FK_Operations_Transfers_SourceTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_SinkId",
                schema: "budget",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_SourceId",
                schema: "budget",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Operations_SinkTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropIndex(
                name: "IX_Operations_SourceTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "SinkTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.DropColumn(
                name: "SourceTransferId",
                schema: "budget",
                table: "Operations");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sle,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,ved,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwg,zwl")
                .OldAnnotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sle,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,ved,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwl");

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SinkId",
                schema: "budget",
                table: "Transfers",
                column: "SinkId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Transfers_SourceId",
                schema: "budget",
                table: "Transfers",
                column: "SourceId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Transfers_SinkId",
                schema: "budget",
                table: "Transfers");

            migrationBuilder.DropIndex(
                name: "IX_Transfers_SourceId",
                schema: "budget",
                table: "Transfers");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sle,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,ved,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwl")
                .OldAnnotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sle,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,ved,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwg,zwl");

            migrationBuilder.AddColumn<Guid>(
                name: "SinkTransferId",
                schema: "budget",
                table: "Operations",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceTransferId",
                schema: "budget",
                table: "Operations",
                type: "uuid",
                nullable: true);

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
    }
}
