using System.ComponentModel.DataAnnotations;

namespace Sannel.House.Sprinklers.Shared.Dtos.Schedules;

/// <summary>
/// DTO for updating an existing schedule program.
/// </summary>
public class UpdateScheduleDto
{
	/// <summary>
	/// The unique identifier of the schedule program to update.
	/// </summary>
	[Required]
	public Guid Id { get; set; }

	/// <summary>
	/// User-defined name for the schedule program.
	/// </summary>
	[Required]
	[MinLength(4)]
	public string Name { get; set; } = string.Empty;

	/// <summary>
	/// The time of day when the schedule starts running.
	/// </summary>
	[Required]
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
	/// A collection of station times defined for the schedule program.
	/// </summary>
	public List<StationTimeDto> StationTimes { get; set; } = new List<StationTimeDto>();
}
