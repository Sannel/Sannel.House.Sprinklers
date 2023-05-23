using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Core.Schedules.Models;

/// <summary>
/// Schedule program class representing a schedule program with a unique identifier, user defined name, 
/// a Cron Expression string and a collection of station times.
/// </summary>
public class ScheduleProgram
{
	/// <summary>
	/// Unique identifier for the schedule program.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// User-defined name for the schedule program.
	/// </summary>
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// Cron Expression for the schedule program.
	/// </summary>
	public string? ScheduleCron { get; set; }

	/// <summary>
	/// Indicates if the schedule program is enabled for sprinkling.
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	/// Collection of station times defined for the schedule program.
	/// </summary>
	public ICollection<StationTime> StationTimes { get; set; } = new List<StationTime>();
}
