using System.ComponentModel.DataAnnotations;

namespace Sannel.House.Sprinklers.Shared.Dtos.Schedules;

/// <summary>
/// Represents a request to update a schedule program.
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
	/// Cron Expression for the schedule program.
	/// </summary>
	[Required]
	[MinLength(5)]
	public string ScheduleCron { get; set; } = string.Empty;

	/// <summary>
	/// A collection of station times defined for the schedule program.
	/// </summary>
	public List<StationTimeDto> StationTimes { get; set; } = new List<StationTimeDto>();
}
