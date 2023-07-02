using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Riok.Mapperly.Abstractions;
using Sannel.House.Sprinklers.Core.Zones;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Core;
[Mapper]
public partial class CoreMapper
{
	public partial ZoneInfoDto ToMessageDto(ZoneMetaData model);
}
