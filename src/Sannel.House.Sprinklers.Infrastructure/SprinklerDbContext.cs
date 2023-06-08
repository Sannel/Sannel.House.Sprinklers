using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sannel.House.Sprinklers.Core.Models;
using Sannel.House.Sprinklers.Core.Schedules.Models;
using Sannel.House.Sprinklers.Core.Zones;

namespace Sannel.House.Sprinklers.Infrastructure;

public class SprinklerDbContext : DbContext
{
	public DbSet<ScheduleProgram> Programs { get; set; }
	public DbSet<StationLog> RunLog { get; set; }

	public DbSet<ZoneRun> Runs { get; set; }

	public DbSet<ZoneMetaData> ZoneMetaDatas { get; set; }

	public SprinklerDbContext(DbContextOptions<SprinklerDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		if (Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite")
		{
			// SQLite does not have proper support for DateTimeOffset via Entity Framework Core, see the limitations
			// here: https://docs.microsoft.com/en-us/ef/core/providers/sqlite/limitations#query-limitations
			// To work around this, when the Sqlite database provider is used, all model properties of type DateTimeOffset
			// use the DateTimeOffsetToBinaryConverter
			// Based on: https://github.com/aspnet/EntityFrameworkCore/issues/10784#issuecomment-415769754
			// This only supports millisecond precision, but should be sufficient for most use cases.
			foreach (var entityType in modelBuilder.Model.GetEntityTypes())
			{
				var properties = entityType.ClrType.GetProperties().Where(p => p.PropertyType == typeof(DateTimeOffset)
																			   || p.PropertyType == typeof(DateTimeOffset?));
				foreach (var property in properties)
				{
					modelBuilder
						.Entity(entityType.Name)
						.Property(property.Name)
						.HasConversion(new DateTimeOffsetToBinaryConverter());
				}
			}
		}
		var program = modelBuilder.Entity<ScheduleProgram>();
		program.HasKey(i => i.Id);
		program.Property(i => i.StationTimes)
			.HasConversion(
				v => JsonSerializer.Serialize(v, JsonSerializerOptions.Default),
				v => JsonSerializer.Deserialize<ICollection<StationTime>>(v, JsonSerializerOptions.Default)
					?? new List<StationTime>()
			);

		var runLog = modelBuilder.Entity<StationLog>();
		runLog.HasKey(i => i.Id);

		var runs = modelBuilder.Entity<ZoneRun>();
		runs.HasKey(nameof(ZoneRun.StartDateTime), nameof(ZoneRun.ZoneId));

		var metaData = modelBuilder.Entity<ZoneMetaData>();
		metaData.HasKey(i => i.ZoneId);
	}
}
