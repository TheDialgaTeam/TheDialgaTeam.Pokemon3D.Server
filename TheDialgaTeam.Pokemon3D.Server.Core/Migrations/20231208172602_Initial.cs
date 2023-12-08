using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TheDialgaTeam.Pokemon3D.Server.Core.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    PlayerType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BannedPlayerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedPlayerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannedPlayerProfiles_PlayerProfiles_PlayerProfileId",
                        column: x => x.PlayerProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockedPlayerProfiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    BlockedProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedPlayerProfiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BlockedPlayerProfiles_PlayerProfiles_BlockedProfileId",
                        column: x => x.BlockedProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlockedPlayerProfiles_PlayerProfiles_PlayerProfileId",
                        column: x => x.PlayerProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalWorlds",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    DoDayCycle = table.Column<bool>(type: "INTEGER", nullable: true),
                    Season = table.Column<int>(type: "INTEGER", nullable: true),
                    Weather = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeOffset = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalWorlds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LocalWorlds_PlayerProfiles_PlayerProfileId",
                        column: x => x.PlayerProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannedPlayerProfiles_PlayerProfileId",
                table: "BannedPlayerProfiles",
                column: "PlayerProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BlockedPlayerProfiles_BlockedProfileId",
                table: "BlockedPlayerProfiles",
                column: "BlockedProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_BlockedPlayerProfiles_PlayerProfileId",
                table: "BlockedPlayerProfiles",
                column: "PlayerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LocalWorlds_PlayerProfileId",
                table: "LocalWorlds",
                column: "PlayerProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_DisplayName",
                table: "PlayerProfiles",
                column: "DisplayName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BannedPlayerProfiles");

            migrationBuilder.DropTable(
                name: "BlockedPlayerProfiles");

            migrationBuilder.DropTable(
                name: "LocalWorlds");

            migrationBuilder.DropTable(
                name: "PlayerProfiles");
        }
    }
}
