using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sannel.House.Sprinklers.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedExtraProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                table: "Runs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartedAt",
                table: "Runs");
        }
    }
}
