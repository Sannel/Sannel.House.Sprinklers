namespace Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;

/// <summary>
/// Represents a request to start a single sprinkler zone for a specified duration.
/// </summary>
public class ZoneStartRequestDto
{
	/// <summary>
	/// The zone identifier to start.
	/// </summary>
	public byte ZoneId { get; set; }

	/// <summary>
	/// The duration to run the zone.
	/// </summary>
	public TimeSpan Length { get; set; }
}
