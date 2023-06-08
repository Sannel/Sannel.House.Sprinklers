namespace Sannel.House.Sprinklers.Core.Zones;
/// <summary>
/// Repository for managing zones.
/// </summary>
public interface IZoneRepository
{
	/// <summary>
	/// Gets the zone information by id.
	/// </summary>
	/// <param name="id">The id of the zone to get information for.</param>
	/// <returns>Zone metadata or null.</returns>
	Task<ZoneMetaData?> GetZoneInfoByIdAsync(byte id);

	/// <summary>
	/// Gets all zone metadata
	/// </summary>
	/// <returns>An <see cref="IEnumerable{ZoneMetaData}"/> object.</returns>
	Task<IEnumerable<ZoneMetaData>> GetAllZones();

	/// <summary>
	/// Adds or updates the zone information.
	/// </summary>
	/// <param name="zoneMetaData">The metadata of the zone to add or update.</param>
	/// <returns>Task.</returns>
	Task AddOrUpdateZoneInfoAsync(ZoneMetaData zoneMetaData);
}
