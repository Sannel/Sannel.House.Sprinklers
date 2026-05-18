using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sannel.House.Sprinklers.Features.Logs;

public class StationLogConfiguration : IEntityTypeConfiguration<StationLog>
{
	public void Configure(EntityTypeBuilder<StationLog> builder)
	{
		builder.HasKey(l => l.Id);
		builder.Ignore(l => l.StationName);
		builder.Ignore(l => l.StationColor);
	}
}
