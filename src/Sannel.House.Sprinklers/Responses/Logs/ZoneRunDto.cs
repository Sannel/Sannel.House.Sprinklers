namespace Sannel.House.Sprinklers.Responses.Logs;

public class ZoneRunDto
{
	/// <summary>
	/// Gets or sets the date and time when this run was executed.
	/// </summary>
	public DateTimeOffset ActionDate { get; set; }

	/// <summary>
	/// Gets or sets the unique identifier for the station.
	/// </summary>
	public byte StationId { get; set; }

	/// <summary>
	/// Gets or sets the duration of the run.
	/// </summary>
	public TimeSpan? RunLength { get; set; }
}
