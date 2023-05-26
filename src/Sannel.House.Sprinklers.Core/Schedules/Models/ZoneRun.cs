using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Core.Schedules.Models;

/// <summary>
/// Represents a zone run for a sprinkler system.
/// </summary>
public class ZoneRun
{
	/// <summary>
	/// The start date and time of the zone run.
	/// </summary>
	public DateTime StartDateTime { get; set; }

	/// <summary>
	/// The ID of the zone being run.
	/// </summary>
	public byte ZoneId { get; set; }

	/// <summary>
	/// Indicates whether or not the zone has been run.
	/// </summary>
	public bool Ran { get; set; }

	/// <summary>
	/// The duration of the zone run.
	/// </summary>
	public TimeSpan RunLength { get; set; }

	/// <summary>
	/// The date and time zone run was started.
	/// </summary>
	public DateTime? StartedAt { get; set; }
}
