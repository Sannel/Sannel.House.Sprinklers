using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Sannel.House.Sprinklers.Core.Models;
using Sannel.House.Sprinklers.Core.Schedules.Models;

namespace Sannel.House.Sprinklers.Infrastructure;

public class SprinklerDbContext : DbContext
{
	public DbSet<ScheduleProgram> Programs { get; set; }
	public DbSet<StationLog> RunLog { get; set; }

	public DbSet<ZoneRun> Runs { get; set; }

	public SprinklerDbContext(DbContextOptions<SprinklerDbContext> options) : base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
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
	}
}
