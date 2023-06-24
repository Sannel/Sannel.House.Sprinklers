using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Shared.Messages;
/// <summary>
/// Represents a message to start a station.
/// </summary>
public class StationStartMessage
{
	/// <summary>
	/// Gets or sets the zone id.
	/// </summary>
	public byte ZoneId { get; set; }

	/// <summary>
	/// Gets or sets the duration of the station start.
	/// </summary>
	public TimeSpan Duration { get; set; } = TimeSpan.Zero;

	/// <summary>
	/// Gets or sets the start time of the station start.
	/// </summary>
	public DateTimeOffset StartTime { get; set; } = DateTimeOffset.Now;
}
