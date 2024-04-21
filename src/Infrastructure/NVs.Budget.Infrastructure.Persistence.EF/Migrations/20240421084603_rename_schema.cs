using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NVs.Budget.Infrastructure.Persistence.EF.Migrations
{
    /// <inheritdoc />
    public partial class rename_schema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "budget");

            migrationBuilder.RenameTable(
                name: "Transfers",
                newName: "Transfers",
                newSchema: "budget");

            migrationBuilder.RenameTable(
                name: "StoredTag",
                newName: "StoredTag",
                newSchema: "budget");

            migrationBuilder.RenameTable(
                name: "StoredAccountStoredOwner",
                newName: "StoredAccountStoredOwner",
                newSchema: "budget");

            migrationBuilder.RenameTable(
                name: "Rates",
                newName: "Rates",
                newSchema: "budget");

            migrationBuilder.RenameTable(
                name: "Owners",
                newName: "Owners",
                newSchema: "budget");

            migrationBuilder.RenameTable(
                name: "Operations",
                newName: "Operations",
                newSchema: "budget");

            migrationBuilder.RenameTable(
                name: "Accounts",
                newName: "Accounts",
                newSchema: "budget");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sle,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,ved,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwl")
                .OldAnnotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "Transfers",
                schema: "budget",
                newName: "Transfers");

            migrationBuilder.RenameTable(
                name: "StoredTag",
                schema: "budget",
                newName: "StoredTag");

            migrationBuilder.RenameTable(
                name: "StoredAccountStoredOwner",
                schema: "budget",
                newName: "StoredAccountStoredOwner");

            migrationBuilder.RenameTable(
                name: "Rates",
                schema: "budget",
                newName: "Rates");

            migrationBuilder.RenameTable(
                name: "Owners",
                schema: "budget",
                newName: "Owners");

            migrationBuilder.RenameTable(
                name: "Operations",
                schema: "budget",
                newName: "Operations");

            migrationBuilder.RenameTable(
                name: "Accounts",
                schema: "budget",
                newName: "Accounts");

            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwl")
                .OldAnnotation("Npgsql:Enum:currency_iso_code", "aed,afn,all,amd,ang,aoa,ars,aud,awg,azn,bam,bbd,bdt,bgn,bhd,bif,bmd,bnd,bob,bov,brl,bsd,btn,bwp,byn,byr,bzd,cad,cdf,che,chf,chw,clf,clp,cny,cop,cou,crc,cuc,cup,cve,czk,djf,dkk,dop,dzd,eek,egp,ern,etb,eur,fjd,fkp,gbp,gel,ghs,gip,gmd,gnf,gtq,gyd,hkd,hnl,hrk,htg,huf,idr,ils,inr,iqd,irr,isk,jmd,jod,jpy,kes,kgs,khr,kmf,kpw,krw,kwd,kyd,kzt,lak,lbp,lkr,lrd,lsl,ltl,lvl,lyd,mad,mdl,mga,mkd,mmk,mnt,mop,mro,mru,mur,mvr,mwk,mxn,mxv,myr,mzn,nad,ngn,nio,nok,npr,nzd,omr,pab,pen,pgk,php,pkr,pln,pyg,qar,ron,rsd,rub,rwf,sar,sbd,scr,sdg,sek,sgd,shp,sle,sll,sos,srd,ssp,std,stn,svc,syp,szl,thb,tjs,tmt,tnd,top,try,ttd,twd,tzs,uah,ugx,usd,usn,uss,uyi,uyu,uyw,uzs,ved,vef,ves,vnd,vuv,wst,xaf,xag,xau,xba,xbb,xbc,xbd,xcd,xdr,xof,xpd,xpf,xpt,xsu,xts,xua,xxx,yer,zar,zmk,zmw,zwl");
        }
    }
}
