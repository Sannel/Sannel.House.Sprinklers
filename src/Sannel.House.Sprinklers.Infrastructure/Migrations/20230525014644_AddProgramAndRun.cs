using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sannel.House.Sprinklers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProgramAndRun : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Runs",
                columns: table => new
                {
                    StartDateTime = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ZoneId = table.Column<byte>(type: "INTEGER", nullable: false),
                    Ran = table.Column<bool>(type: "INTEGER", nullable: false),
                    RunLength = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Runs", x => new { x.StartDateTime, x.ZoneId });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Runs");
        }
    }
}
