using System.ComponentModel.DataAnnotations;

namespace Sannel.House.Sprinklers.Shared.Dtos.Schedules;

/// <summary>
/// Schedule program DTO class representing a schedule program with a unique identifier, user defined name, 
/// a Cron Expression string and a collection of station times.
/// </summary>
public class NewScheduleDto
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
	public List<StationTimeDto> StationTimes { get; set; } = new List<StationTimeDto>();
}
