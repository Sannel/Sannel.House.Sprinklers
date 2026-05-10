using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sannel.House.Sprinklers.Features.Zones;

public class ZoneMetaDataConfiguration : IEntityTypeConfiguration<ZoneMetaData>
{
	public void Configure(EntityTypeBuilder<ZoneMetaData> builder)
	{
		builder.HasKey(z => z.ZoneId);
	}
}
