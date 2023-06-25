using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared.Messages;
public class StationProgressMessage
{
	public byte ZoneId { get; set; }

	public TimeSpan TimeLeft { get; set; } = TimeSpan.Zero;
	public TimeSpan RunLength { get; set; } = TimeSpan.Zero;

	public float PercentCompleteFloat { get; set; } = 0.0f;
	public short PercentComplete { get; set; }
	public short InvertPercentComplete { get; set; }
	public DateTimeOffset TriggerTime { get; set; }
}
