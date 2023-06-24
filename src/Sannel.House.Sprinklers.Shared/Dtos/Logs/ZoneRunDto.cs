using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Shared.Dtos;

/// <summary>
/// Represents data about a sprinkler zone run.
/// </summary>
public class ZoneRunDto
{
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

	/// <summary>
	/// Gets or sets the name of the station.
	/// </summary>
	public string? StationName { get; set; }

	/// <summary>
	/// Gets or sets the color of the station.
	/// </summary>
	public string? StationColor { get; set; }
}
