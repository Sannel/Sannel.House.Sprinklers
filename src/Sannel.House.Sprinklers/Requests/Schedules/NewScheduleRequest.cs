using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Core.Schedules.Models;

namespace Sannel.House.Sprinklers.Requests.Schedules;

/// <summary>
/// Schedule program DTO class representing a schedule program with a unique identifier, user defined name, 
/// a Cron Expression string and a collection of station times.
/// </summary>
public class NewScheduleRequest
{

	/// <summary>
	/// User-defined name for the schedule program.
	/// </summary>
	[Required]
	[MinLength(4)]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Cron Expression for the schedule program.
	/// </summary>
	[Required]
	[MinLength(5)]
	public string ScheduleCron { get; set; } = string.Empty;

	/// <summary>
	/// Collection of station times defined for the schedule program.
	/// </summary>
	public List<StationTime> StationTimes { get; set; } = new List<StationTime>();
}
