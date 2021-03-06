// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheDialgaTeam.Pokemon3D.Server.Database;

namespace TheDialgaTeam.Pokemon3D.Server.Migrations
{
    [DbContext(typeof(SqliteDatabaseContext))]
    [Migration("20210509152319_1.0.0.0")]
    partial class _1000
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.5");

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Database.Tables.Blacklist", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameJoltId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Duration")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Name", "GameJoltId");

                    b.ToTable("Blacklists");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Database.Tables.Mutelist", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameJoltId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Duration")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Name", "GameJoltId");

                    b.ToTable("Mutelists");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Database.Tables.Operator", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameJoltId")
                        .HasColumnType("TEXT");

                    b.Property<int>("Level")
                        .HasColumnType("INTEGER");

                    b.HasKey("Name", "GameJoltId");

                    b.ToTable("Operators");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Database.Tables.Whitelist", b =>
                {
                    b.Property<string>("Name")
                        .HasColumnType("TEXT");

                    b.Property<string>("GameJoltId")
                        .HasColumnType("TEXT");

                    b.HasKey("Name", "GameJoltId");

                    b.ToTable("Whitelists");
                });
#pragma warning restore 612, 618
        }
    }
}
