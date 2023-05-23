using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Core.Models;

/// <summary>
/// Represents a single run for a station.
/// </summary>
public class StationLog
{
	/// <summary>
	/// Gets or sets the unique identifier for this run.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Gets or sets the action performed during this run.
	/// </summary>
	public string Action { get; set; } = "Unknown";

	/// <summary>
	/// Gets or sets the date and time when this run was executed.
	/// </summary>
	public DateTimeOffset ActionDate { get; set; }

	/// <summary>
	/// Gets or sets the unique identifier for the station.
	/// </summary>
	public byte StationId { get; set; }

	/// <summary>
	/// Gets or sets the duration of the run.
	/// </summary>
	public TimeSpan? RunLength { get; set; }
}
