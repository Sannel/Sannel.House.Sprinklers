using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sannel.House.Sprinklers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddStationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RunLog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    ActionDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    StationId = table.Column<byte>(type: "INTEGER", nullable: false),
                    RunLength = table.Column<TimeSpan>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RunLog", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RunLog");
        }
    }
}
