namespace Sannel.House.Sprinklers.Features.Schedules;

/// <summary>
/// A schedule program that triggers zone runs using a hybrid scheduling model —
/// either on specific days of the week or on a fixed-day interval.
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
	/// The time of day when the schedule starts running.
	/// </summary>
	public TimeOnly StartTime { get; set; }

	/// <summary>
	/// Days of the week on which the schedule runs.
	/// Mutually exclusive with <see cref="IntervalDays"/>.
	/// </summary>
	public ICollection<DayOfWeek>? DaysOfWeek { get; set; }

	/// <summary>
	/// Number of days between each run in interval mode.
	/// Mutually exclusive with <see cref="DaysOfWeek"/>.
	/// </summary>
	public int? IntervalDays { get; set; }

	/// <summary>
	/// The reference start date used to calculate interval runs.
	/// Required when <see cref="IntervalDays"/> is set.
	/// </summary>
	public DateOnly? IntervalStartDate { get; set; }

	/// <summary>
	/// Indicates if the schedule program is enabled for sprinkling.
	/// </summary>
	public bool Enabled { get; set; }

	/// <summary>
	/// Collection of station times defined for the schedule program.
	/// </summary>
	public ICollection<StationTime> StationTimes { get; set; } = new List<StationTime>();
}
