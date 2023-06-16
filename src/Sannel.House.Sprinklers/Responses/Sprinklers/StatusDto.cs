using Sannel.House.Sprinklers.Responses.Zones;

namespace Sannel.House.Sprinklers.Responses.Sprinklers;

/// <summary>
/// Represents the status of a sprinkler system.
/// </summary>
public class StatusDto
{
	/// <summary>
	/// Gets or sets a value indicating whether the sprinkler system is running.
	/// </summary>
	public bool IsRunning { get; set; }

	/// <summary>
	/// Gets or sets the time left for the sprinkler zone to continue running.
	/// </summary>
	public TimeSpan? TimeLeft { get; set; }

	/// <summary>
	/// Gets or sets the total time the sprinkler system has been running.
	/// </summary>
	public TimeSpan? TotalTime { get; set; }

	/// <summary>
	/// Gets or sets the number of zones in the sprinkler system.
	/// </summary>
	public byte Zones { get; set; }

	/// <summary>
	/// Gets or sets information about the running zone.
	/// </summary>
	public ZoneInfoDto? ZoneInfo { get; set; }
}
