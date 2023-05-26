using Sannel.House.Sprinklers.Core.Schedules.Models;

namespace Sannel.House.Sprinklers.Responses.Schedules;

public class ScheduleProgramResponse
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
