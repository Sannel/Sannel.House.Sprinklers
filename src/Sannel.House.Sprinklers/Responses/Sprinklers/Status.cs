namespace Sannel.House.Sprinklers.Responses.Sprinklers;

/// <summary>
/// Represents the status of a sprinkler system.
/// </summary>
public class Status
{
	/// <summary>
	/// Gets or sets a value indicating whether the sprinkler system is running.
	/// </summary>
	public bool IsRunning { get; set; }

	/// <summary>
	/// Gets or sets the running station number of the sprinkler system.
	/// </summary>
	public byte? RunningZone { get; set; }

	/// <summary>
	/// Gets or sets the time left for the sprinkler zone to continue running.
	/// </summary>
	public TimeSpan TimeLeft { get; set; }

	public byte Zones { get; set; }
}
