namespace Sannel.House.Sprinklers.Core.Zones;
/// <summary>
/// Represents the metadata of a sprinkler zone.
/// </summary>
public class ZoneMetaData
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
