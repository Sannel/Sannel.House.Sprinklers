
namespace Sannel.House.Sprinklers.Shared.Dtos.Schedules;

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
	public ICollection<StationTimeDto> StationTimes { get; set; } = new List<StationTimeDto>();
}
