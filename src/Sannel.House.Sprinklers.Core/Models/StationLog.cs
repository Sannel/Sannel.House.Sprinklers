namespace Sannel.House.Sprinklers.Core.Models;

/// <summary>
/// Represents a single run for a station.
/// </summary>
public class StationLog
{
	/// <summary>
	/// Gets or sets the unique identifier for this run.
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// Gets or sets the action performed during this run.
	/// </summary>
	public string Action { get; set; } = "Unknown";

	/// <summary>
	/// Gets or sets the date and time when this run was executed.
	/// </summary>
	public DateTimeOffset ActionDate { get; set; }

	/// <summary>
	/// Gets or sets the unique identifier for the Zone.
	/// </summary>
	public byte ZoneId { get; set; }

	/// <summary>
	/// Gets or sets the duration of the run.
	/// </summary>
	public TimeSpan? RunLength { get; set; }

	/// <summary>
	/// Gets or set the username that initiated this action.
	/// </summary>
	public string? Username { get; set; }

	/// <summary>
	/// Gets or sets the unique identifier of the user that initiated this action.
	/// </summary>
	public string? UserId { get; set; }

	/// <summary>
	/// Gets or sets the station name for this log entry.
	/// </summary>
	public string? StationName { get; set; }

	/// <summary>
	/// Gets or sets the color for the station that corresponds to this log entry.
	/// </summary>
	public string? StationColor { get; set; }
}
