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
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 512, nullable: false),
                    GameJoltId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Password = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    PlayerType = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerProfiles", x => x.PlayerProfileId);
                });

            migrationBuilder.CreateTable(
                name: "BannedPlayerProfiles",
                columns: table => new
                {
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Reason = table.Column<string>(type: "TEXT", maxLength: 512, nullable: true),
                    StartTime = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Duration = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedPlayerProfiles", x => x.PlayerProfileId);
                    table.ForeignKey(
                        name: "FK_BannedPlayerProfiles_PlayerProfiles_PlayerProfileId",
                        column: x => x.PlayerProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "PlayerProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BlockedPlayerProfiles",
                columns: table => new
                {
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false),
                    BlockedProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlockedPlayerProfiles", x => new { x.PlayerProfileId, x.BlockedProfileId });
                    table.ForeignKey(
                        name: "FK_BlockedPlayerProfiles_PlayerProfiles_BlockedProfileId",
                        column: x => x.BlockedProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "PlayerProfileId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlockedPlayerProfiles_PlayerProfiles_PlayerProfileId",
                        column: x => x.PlayerProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "PlayerProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LocalWorlds",
                columns: table => new
                {
                    PlayerProfileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DoDayCycle = table.Column<bool>(type: "INTEGER", nullable: true),
                    Season = table.Column<int>(type: "INTEGER", nullable: true),
                    Weather = table.Column<int>(type: "INTEGER", nullable: true),
                    TimeOffset = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LocalWorlds", x => x.PlayerProfileId);
                    table.ForeignKey(
                        name: "FK_LocalWorlds_PlayerProfiles_PlayerProfileId",
                        column: x => x.PlayerProfileId,
                        principalTable: "PlayerProfiles",
                        principalColumn: "PlayerProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlockedPlayerProfiles_BlockedProfileId",
                table: "BlockedPlayerProfiles",
                column: "BlockedProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_DisplayName",
                table: "PlayerProfiles",
                column: "DisplayName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerProfiles_GameJoltId",
                table: "PlayerProfiles",
                column: "GameJoltId");
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
