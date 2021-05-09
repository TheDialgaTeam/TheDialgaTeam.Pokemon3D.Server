using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheDialgaTeam.Pokemon3D.Server.Migrations
{
    public partial class _1000 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blacklists",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklists", x => new { x.Name, x.GameJoltId });
                });

            migrationBuilder.CreateTable(
                name: "Mutelists",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mutelists", x => new { x.Name, x.GameJoltId });
                });

            migrationBuilder.CreateTable(
                name: "Operators",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<string>(type: "TEXT", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Operators", x => new { x.Name, x.GameJoltId });
                });

            migrationBuilder.CreateTable(
                name: "Whitelists",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Whitelists", x => new { x.Name, x.GameJoltId });
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blacklists");

            migrationBuilder.DropTable(
                name: "Mutelists");

            migrationBuilder.DropTable(
                name: "Operators");

            migrationBuilder.DropTable(
                name: "Whitelists");
        }
    }
}
