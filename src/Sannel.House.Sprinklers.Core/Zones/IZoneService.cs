namespace Sannel.House.Sprinklers.Core.Zones;

public interface IZoneService
{
	Task AddOrUpdateZoneInfoAsync(ZoneMetaData zoneMetaData);
	Task<IEnumerable<ZoneMetaData>> GetAllZones();
	Task<ZoneMetaData?> GetZoneInfoByIdAsync(byte id);
}