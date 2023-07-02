using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Shared.Messages;
public class ZoneUpdateMessage
{
	public required ZoneInfoDto ZoneInfo { get; set; }
	public required DateTimeOffset UpdateTime { get; set; } = DateTimeOffset.Now;
}
