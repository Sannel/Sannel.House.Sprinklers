using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class ScheduleProgramConfiguration : IEntityTypeConfiguration<ScheduleProgram>
{
	public void Configure(EntityTypeBuilder<ScheduleProgram> builder)
	{
		builder.HasKey(s => s.Id);
		builder.Property(s => s.StationTimes)
			.HasConversion(
				v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
				v => JsonSerializer.Deserialize<ICollection<StationTime>>(v, (JsonSerializerOptions?)null)!);
		builder.Property(s => s.DaysOfWeek)
			.HasConversion(
				v => v != null ? JsonSerializer.Serialize(v, (JsonSerializerOptions?)null) : null,
				v => v != null ? JsonSerializer.Deserialize<ICollection<DayOfWeek>>(v, (JsonSerializerOptions?)null) : null);
	}
}
