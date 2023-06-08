using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Core.Zones;

namespace Sannel.House.Sprinklers.Infrastructure.Zones;
/// <summary>
/// Implements IZoneRepository interface
/// </summary>
public class ZoneRepository : IZoneRepository
{
	private readonly SprinklerDbContext _context;

	/// <summary>
	/// Initializes a new instance 
	/// </summary>
	/// <param name="context">SprinklerDbContext instance</param>
	public ZoneRepository(SprinklerDbContext context)
	{
		ArgumentNullException.ThrowIfNull(context);
		_context = context;
	}

	/// <inheritdoc/>
	public async Task AddOrUpdateZoneInfoAsync(ZoneMetaData zoneMetaData)
	{
		var zone = await _context.ZoneMetaDatas.FirstOrDefaultAsync(i => i.ZoneId == zoneMetaData.ZoneId);
		if (zone is null)
		{
			zone = new ZoneMetaData()
			{ ZoneId = zoneMetaData.ZoneId };
			await _context.ZoneMetaDatas.AddAsync(zone);
		}

		zone.Name = zoneMetaData.Name;
		zone.Color = zoneMetaData.Color;
		await _context.SaveChangesAsync();
	}

	/// <inheritdoc/>
	public async Task<IEnumerable<ZoneMetaData>> GetAllZones()
	{
		return await _context.ZoneMetaDatas.AsNoTracking().OrderBy(i => i.ZoneId).ToListAsync();
	}

	/// <inheritdoc/>
	public async Task<ZoneMetaData?> GetZoneInfoByIdAsync(byte id)
	{
		var zone = await _context.ZoneMetaDatas.AsNoTracking().FirstOrDefaultAsync(i => i.ZoneId == id);

		return zone;
	}
}
