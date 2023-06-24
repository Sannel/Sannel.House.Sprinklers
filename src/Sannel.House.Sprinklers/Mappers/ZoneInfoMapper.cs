using Riok.Mapperly.Abstractions;
using Sannel.House.Sprinklers.Core.Zones;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Mappers;

[Mapper]
public partial class ZoneInfoMapper
{
	public partial ZoneInfoDto ModelToDto(ZoneMetaData zoneInfo);
	public partial ZoneMetaData DtoToModel(ZoneInfoDto zoneInfo);
}
