using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Core.Schedules.Models;

/// <summary>
/// Represents the time that a sprinkler station should run.
/// </summary>
public class StationTime
{
	/// <summary>
	/// Gets or sets the ID of the station.
	/// </summary>
	public byte StationId { get; set; }

	/// <summary>
	/// Gets or sets the length of time that the station should run.
	/// </summary>
	public TimeSpan RunLength { get; set; }
}
