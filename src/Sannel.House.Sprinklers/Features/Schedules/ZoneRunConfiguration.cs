using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class ZoneRunConfiguration : IEntityTypeConfiguration<ZoneRun>
{
	public void Configure(EntityTypeBuilder<ZoneRun> builder)
	{
		builder.HasKey(r => new { r.StartDateTime, r.ZoneId });
	}
}
