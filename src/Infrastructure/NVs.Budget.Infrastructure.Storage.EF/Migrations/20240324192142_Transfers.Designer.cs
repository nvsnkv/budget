﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NVs.Budget.Infrastructure.Storage.Context;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace NVs.Budget.Infrastructure.Storage.Migrations
{
    [DbContext(typeof(BudgetContext))]
    [Migration("20240324192142_Transfers")]
    partial class Transfers
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.HasPostgresEnum(modelBuilder, "currency_iso_code", new[] { "aed", "afn", "all", "amd", "ang", "aoa", "ars", "aud", "awg", "azn", "bam", "bbd", "bdt", "bgn", "bhd", "bif", "bmd", "bnd", "bob", "bov", "brl", "bsd", "btn", "bwp", "byn", "byr", "bzd", "cad", "cdf", "che", "chf", "chw", "clf", "clp", "cny", "cop", "cou", "crc", "cuc", "cup", "cve", "czk", "djf", "dkk", "dop", "dzd", "eek", "egp", "ern", "etb", "eur", "fjd", "fkp", "gbp", "gel", "ghs", "gip", "gmd", "gnf", "gtq", "gyd", "hkd", "hnl", "hrk", "htg", "huf", "idr", "ils", "inr", "iqd", "irr", "isk", "jmd", "jod", "jpy", "kes", "kgs", "khr", "kmf", "kpw", "krw", "kwd", "kyd", "kzt", "lak", "lbp", "lkr", "lrd", "lsl", "ltl", "lvl", "lyd", "mad", "mdl", "mga", "mkd", "mmk", "mnt", "mop", "mro", "mru", "mur", "mvr", "mwk", "mxn", "mxv", "myr", "mzn", "nad", "ngn", "nio", "nok", "npr", "nzd", "omr", "pab", "pen", "pgk", "php", "pkr", "pln", "pyg", "qar", "ron", "rsd", "rub", "rwf", "sar", "sbd", "scr", "sdg", "sek", "sgd", "shp", "sll", "sos", "srd", "ssp", "std", "stn", "svc", "syp", "szl", "thb", "tjs", "tmt", "tnd", "top", "try", "ttd", "twd", "tzs", "uah", "ugx", "usd", "usn", "uss", "uyi", "uyu", "uyw", "uzs", "vef", "ves", "vnd", "vuv", "wst", "xaf", "xag", "xau", "xba", "xbb", "xbc", "xbd", "xcd", "xdr", "xof", "xpd", "xpf", "xpt", "xsu", "xts", "xua", "xxx", "yer", "zar", "zmk", "zmw", "zwl" });
            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredAccount", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Bank")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Deleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Version")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredOperation", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("uuid");

                    b.Property<string>("Attributes")
                        .IsRequired()
                        .HasColumnType("jsonb");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Deleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("SinkTransferId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("SourceTransferId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("Timestamp")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Version")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.HasIndex("AccountId");

                    b.HasIndex("SinkTransferId");

                    b.HasIndex("SourceTransferId");

                    b.ToTable("Operations");
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredOwner", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Deleted")
                        .HasColumnType("boolean");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.ToTable("Owners");
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredRate", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime>("AsOf")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Deleted")
                        .HasColumnType("boolean");

                    b.Property<int>("From")
                        .HasColumnType("integer");

                    b.Property<Guid>("OwnerId")
                        .HasColumnType("uuid");

                    b.Property<decimal>("Rate")
                        .HasColumnType("numeric");

                    b.Property<int>("To")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("OwnerId");

                    b.ToTable("Rates");
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredTransfer", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Comment")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<bool>("Deleted")
                        .HasColumnType("boolean");

                    b.Property<Guid>("SinkId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("SourceId")
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("UpdatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("Id");

                    b.HasIndex("SinkId");

                    b.HasIndex("SourceId");

                    b.ToTable("Transfers");
                });

            modelBuilder.Entity("StoredAccountStoredOwner", b =>
                {
                    b.Property<Guid>("AccountsId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("OwnersId")
                        .HasColumnType("uuid");

                    b.HasKey("AccountsId", "OwnersId");

                    b.HasIndex("OwnersId");

                    b.ToTable("StoredAccountStoredOwner");
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredOperation", b =>
                {
                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredAccount", "Account")
                        .WithMany("Operations")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredTransfer", "SinkTransfer")
                        .WithMany()
                        .HasForeignKey("SinkTransferId");

                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredTransfer", "SourceTransfer")
                        .WithMany()
                        .HasForeignKey("SourceTransferId");

                    b.OwnsOne("NVs.Budget.Infrastructure.Storage.Entities.StoredMoney", "Amount", b1 =>
                        {
                            b1.Property<Guid>("StoredOperationId")
                                .HasColumnType("uuid");

                            b1.Property<decimal>("Amount")
                                .HasColumnType("numeric");

                            b1.Property<int>("Currency")
                                .HasColumnType("integer");

                            b1.HasKey("StoredOperationId");

                            b1.ToTable("Operations");

                            b1.WithOwner()
                                .HasForeignKey("StoredOperationId");
                        });

                    b.OwnsMany("NVs.Budget.Infrastructure.Storage.Entities.StoredTag", "Tags", b1 =>
                        {
                            b1.Property<Guid>("StoredOperationId")
                                .HasColumnType("uuid");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<string>("Value")
                                .IsRequired()
                                .HasColumnType("text");

                            b1.HasKey("StoredOperationId", "Id");

                            b1.ToTable("StoredTag");

                            b1.WithOwner()
                                .HasForeignKey("StoredOperationId");
                        });

                    b.Navigation("Account");

                    b.Navigation("Amount")
                        .IsRequired();

                    b.Navigation("SinkTransfer");

                    b.Navigation("SourceTransfer");

                    b.Navigation("Tags");
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredRate", b =>
                {
                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredOwner", "Owner")
                        .WithMany()
                        .HasForeignKey("OwnerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Owner");
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredTransfer", b =>
                {
                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredOperation", "Sink")
                        .WithMany()
                        .HasForeignKey("SinkId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredOperation", "Source")
                        .WithMany()
                        .HasForeignKey("SourceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsOne("NVs.Budget.Infrastructure.Storage.Entities.StoredMoney", "Fee", b1 =>
                        {
                            b1.Property<Guid>("StoredTransferId")
                                .HasColumnType("uuid");

                            b1.Property<decimal>("Amount")
                                .HasColumnType("numeric");

                            b1.Property<int>("Currency")
                                .HasColumnType("integer");

                            b1.HasKey("StoredTransferId");

                            b1.ToTable("Transfers");

                            b1.WithOwner()
                                .HasForeignKey("StoredTransferId");
                        });

                    b.Navigation("Fee")
                        .IsRequired();

                    b.Navigation("Sink");

                    b.Navigation("Source");
                });

            modelBuilder.Entity("StoredAccountStoredOwner", b =>
                {
                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredAccount", null)
                        .WithMany()
                        .HasForeignKey("AccountsId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("NVs.Budget.Infrastructure.Storage.Entities.StoredOwner", null)
                        .WithMany()
                        .HasForeignKey("OwnersId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("NVs.Budget.Infrastructure.Storage.Entities.StoredAccount", b =>
                {
                    b.Navigation("Operations");
                });
#pragma warning restore 612, 618
        }
    }
}
