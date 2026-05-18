using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sannel.House.Sprinklers.Features.Logs;
using Sannel.House.Sprinklers.Features.Schedules;
using Sannel.House.Sprinklers.Features.Zones;

namespace Sannel.House.Sprinklers.Features.Common;

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
		base.OnModelCreating(modelBuilder);

		modelBuilder.ApplyConfigurationsFromAssembly(typeof(SprinklerDbContext).Assembly);

		var converter = new ValueConverter<DateTimeOffset, long>(
			v => v.ToFileTime(),
			v => DateTimeOffset.FromFileTime(v));

		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			foreach (var property in entityType.GetProperties())
			{
				if (property.ClrType == typeof(DateTimeOffset) || property.ClrType == typeof(DateTimeOffset?))
				{
					property.SetValueConverter(converter);
				}
			}
		}
	}
}
