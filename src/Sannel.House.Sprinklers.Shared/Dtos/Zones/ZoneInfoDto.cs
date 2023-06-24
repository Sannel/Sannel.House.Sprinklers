namespace Sannel.House.Sprinklers.Shared.Dtos.Zones;
/// <summary>
/// Represents information about a sprinkler zone.
/// </summary>
public class ZoneInfoDto
{
	/// <summary>
	/// Gets or sets the zone ID.
	/// </summary>
	public byte ZoneId { get; set; }

	/// <summary>
	/// Gets or sets the name of the zone.
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// Gets or sets the color of the zone.
	/// </summary>
	public string? Color { get; set; }

}
