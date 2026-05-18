using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sannel.House.Sprinklers.Features.Common.Migrations
{
    /// <inheritdoc />
    public partial class HybridScheduling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ScheduleCron",
                table: "Programs",
                newName: "IntervalStartDate");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ZoneMetaDatas",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AddColumn<string>(
                name: "DaysOfWeek",
                table: "Programs",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntervalDays",
                table: "Programs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "StartTime",
                table: "Programs",
                type: "TEXT",
                nullable: false,
                defaultValue: new TimeOnly(0, 0, 0));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DaysOfWeek",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "IntervalDays",
                table: "Programs");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Programs");

            migrationBuilder.RenameColumn(
                name: "IntervalStartDate",
                table: "Programs",
                newName: "ScheduleCron");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "ZoneMetaDatas",
                type: "TEXT",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);
        }
    }
}
