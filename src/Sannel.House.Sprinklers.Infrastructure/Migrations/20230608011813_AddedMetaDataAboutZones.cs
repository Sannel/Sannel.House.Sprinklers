using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sannel.House.Sprinklers.Infrastructure.Migrations
{
	/// <inheritdoc />
	public partial class AddedMetaDataAboutZones : Migration
	{
		/// <inheritdoc />
		protected override void Up(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.RenameColumn(
				name: "StationId",
				table: "RunLog",
				newName: "ZoneId");

			migrationBuilder.AlterColumn<long>(
				name: "ActionDate",
				table: "RunLog",
				type: "INTEGER",
				nullable: false,
				oldClrType: typeof(DateTimeOffset),
				oldType: "TEXT");

			migrationBuilder.AddColumn<string>(
				name: "UserId",
				table: "RunLog",
				type: "TEXT",
				nullable: true);

			migrationBuilder.AddColumn<string>(
				name: "Username",
				table: "RunLog",
				type: "TEXT",
				nullable: true);

			migrationBuilder.CreateTable(
				name: "ZoneMetaDatas",
				columns: table => new
				{
					ZoneId = table.Column<byte>(type: "INTEGER", nullable: false),
					Name = table.Column<string>(type: "TEXT", nullable: true),
					Color = table.Column<string>(type: "TEXT", nullable: true)
				},
				constraints: table =>
				{
					table.PrimaryKey("PK_ZoneMetaDatas", x => x.ZoneId);
				});
		}

		/// <inheritdoc />
		protected override void Down(MigrationBuilder migrationBuilder)
		{
			migrationBuilder.DropTable(
				name: "ZoneMetaDatas");

			migrationBuilder.DropColumn(
				name: "UserId",
				table: "RunLog");

			migrationBuilder.DropColumn(
				name: "Username",
				table: "RunLog");

			migrationBuilder.RenameColumn(
				name: "ZoneId",
				table: "RunLog",
				newName: "StationId");

			migrationBuilder.AlterColumn<DateTimeOffset>(
				name: "ActionDate",
				table: "RunLog",
				type: "TEXT",
				nullable: false,
				oldClrType: typeof(long),
				oldType: "INTEGER");
		}
	}
}
