using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared.Messages;
public class StationStopMessage
{
	public byte ZoneId { get; set; }
	public DateTimeOffset StopTime { get; set; } = DateTimeOffset.Now;
}
