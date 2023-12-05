﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TheDialgaTeam.Pokemon3D.Server.Core.Database;

#nullable disable

namespace TheDialgaTeam.Pokemon3D.Server.Core.Migrations
{
    [DbContext(typeof(DatabaseContext))]
    partial class DatabaseContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.0");

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.Blacklist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<TimeSpan>("Duration")
                        .HasColumnType("TEXT");

                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Reason")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<DateTimeOffset>("StartTime")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("BlacklistAccounts");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.LocalWorld", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool?>("DoDayCycle")
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerProfileId")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Season")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("TimeOffset")
                        .HasColumnType("INTEGER");

                    b.Property<int?>("Weather")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayerProfileId")
                        .IsUnique();

                    b.ToTable("LocalWorldSettings");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.PlayerProfile", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("GameJoltId")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("TEXT");

                    b.Property<int>("PlayerType")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.ToTable("PlayerProfiles");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.Whitelist", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("PlayerId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("PlayerId");

                    b.ToTable("WhitelistAccounts");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.Blacklist", b =>
                {
                    b.HasOne("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.PlayerProfile", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.LocalWorld", b =>
                {
                    b.HasOne("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.PlayerProfile", "PlayerProfile")
                        .WithOne("LocalWorld")
                        .HasForeignKey("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.LocalWorld", "PlayerProfileId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PlayerProfile");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.Whitelist", b =>
                {
                    b.HasOne("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.PlayerProfile", "Player")
                        .WithMany()
                        .HasForeignKey("PlayerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Player");
                });

            modelBuilder.Entity("TheDialgaTeam.Pokemon3D.Server.Core.Database.Tables.PlayerProfile", b =>
                {
                    b.Navigation("LocalWorld")
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
