namespace Sannel.House.Sprinklers.Shared.Dtos.Schedules;

/// <summary>
/// Represents a schedule program with hybrid scheduling (day-of-week or interval).
/// </summary>
public class ScheduleProgramDto
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
	/// The time of day when the schedule starts running.
	/// </summary>
	public TimeOnly StartTime { get; set; }

	/// <summary>
	/// Days of the week on which the schedule runs.
	/// Null when using interval mode.
	/// </summary>
	public ICollection<DayOfWeek>? DaysOfWeek { get; set; }

	/// <summary>
	/// Number of days between each run in interval mode.
	/// Null when using day-of-week mode.
	/// </summary>
	public int? IntervalDays { get; set; }

	/// <summary>
	/// The reference start date used to calculate interval runs.
	/// </summary>
	public DateOnly? IntervalStartDate { get; set; }

	/// <summary>
	/// Indicates if the schedule program is enabled for sprinkling.
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	/// Collection of station times defined for the schedule program.
	/// </summary>
	public ICollection<StationTimeDto> StationTimes { get; set; } = new List<StationTimeDto>();
}
