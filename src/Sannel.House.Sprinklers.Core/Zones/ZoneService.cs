using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Core.Zones;
public class ZoneService : IZoneService
{
	private readonly IZoneRepository _zoneRepository;
	private readonly IMessageClient _messageClient;
	private readonly CoreMapper _mapper;

	public ZoneService(IZoneRepository zoneRepository,
		IMessageClient messageClient,
		CoreMapper mapper)
	{
		ArgumentNullException.ThrowIfNull(zoneRepository);
		ArgumentNullException.ThrowIfNull(messageClient);
		ArgumentNullException.ThrowIfNull(mapper);
		_zoneRepository = zoneRepository;
		_messageClient = messageClient;
		_mapper = mapper;
	}

	/// <summary>
	/// Gets the zone information by id.
	/// </summary>
	/// <param name="id">The id of the zone to get information for.</param>
	/// <returns>Zone metadata or null.</returns>
	public Task<ZoneMetaData?> GetZoneInfoByIdAsync(byte id)
		=> _zoneRepository.GetZoneInfoByIdAsync(id);

	/// <summary>
	/// Gets all zone metadata
	/// </summary>
	/// <returns>An <see cref="IEnumerable{ZoneMetaData}"/> object.</returns>
	public Task<IEnumerable<ZoneMetaData>> GetAllZones()
		=> _zoneRepository.GetAllZones();

	/// <summary>
	/// Adds or updates the zone information.
	/// </summary>
	/// <param name="zoneMetaData">The metadata of the zone to add or update.</param>
	/// <returns>Task.</returns>
	public async Task AddOrUpdateZoneInfoAsync(ZoneMetaData zoneMetaData)
	{
		await _zoneRepository.AddOrUpdateZoneInfoAsync(zoneMetaData);
		var z = await _zoneRepository.GetZoneInfoByIdAsync(zoneMetaData.ZoneId);
		if (z is not null)
		{
			await _messageClient.SendZoneUpdateMessageAsync(new Shared.Messages.ZoneUpdateMessage()
			{
				ZoneInfo = _mapper.ToMessageDto(z),
				UpdateTime = DateTimeOffset.Now
			});
		}
	}
}
