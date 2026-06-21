using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sannel.House.Sprinklers.Features.Common.Migrations
{
    /// <inheritdoc />
    public partial class MigrateZoneIdsToOneBased : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Zone IDs were originally 0-based. Commit 00b41c2 added a -1 conversion in
            // SprinklerWorker so ZoneIds became 1-based externally. This migration fixes
            // any schedule/run data that was stored under the old 0-based scheme.

            // 1. Re-number StationTimes JSON in Programs that still reference StationId=0.
            //    For those programs every StationId is incremented by 1 (0-based → 1-based).
            migrationBuilder.Sql(@"
                UPDATE Programs
                SET StationTimes = (
                    SELECT json_group_array(
                        json_object(
                            'StationId', CAST(json_extract(entry.value, '$.StationId') AS INTEGER) + 1,
                            'RunLength', json_extract(entry.value, '$.RunLength')
                        )
                    )
                    FROM json_each(Programs.StationTimes) AS entry
                )
                WHERE EXISTS (
                    SELECT 1
                    FROM json_each(Programs.StationTimes) AS jt
                    WHERE CAST(json_extract(jt.value, '$.StationId') AS INTEGER) = 0
                );
            ");

            // 2. Drop all unrun ZoneRun records. The ScheduleWorker will regenerate
            //    today's (and future) runs from the now-corrected schedule programs.
            migrationBuilder.Sql("DELETE FROM Runs WHERE Ran = 0;");

            // 3. Fix ZoneMetaData records with ZoneId=0 (custom name/colour for zone 0).
            //    If ZoneId=1 already exists we can't rename, so just drop the orphaned row.
            migrationBuilder.Sql(@"
                UPDATE ZoneMetaDatas SET ZoneId = 1
                WHERE ZoneId = 0
                  AND NOT EXISTS (SELECT 1 FROM ZoneMetaDatas WHERE ZoneId = 1);
            ");
            migrationBuilder.Sql("DELETE FROM ZoneMetaDatas WHERE ZoneId = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverting a data-only migration is not practical; a full restore from backup
            // would be required if a rollback is needed.
        }
    }
}
