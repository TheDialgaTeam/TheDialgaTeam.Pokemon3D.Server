using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TheDialgaTeam.Pokemon3D.Server.Migrations
{
    public partial class _0100 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Blacklists",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blacklists", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "IpBlacklists",
                columns: table => new
                {
                    IPAddress = table.Column<string>(type: "TEXT", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IpBlacklists", x => x.IPAddress);
                });

            migrationBuilder.CreateTable(
                name: "MuteLists",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<int>(type: "INTEGER", nullable: false),
                    Reason = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Duration = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MuteLists", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "OperatorLists",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<int>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorLists", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Whitelists",
                columns: table => new
                {
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    GameJoltId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Whitelists", x => x.Name);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Blacklists");

            migrationBuilder.DropTable(
                name: "IpBlacklists");

            migrationBuilder.DropTable(
                name: "MuteLists");

            migrationBuilder.DropTable(
                name: "OperatorLists");

            migrationBuilder.DropTable(
                name: "Whitelists");
        }
    }
}
