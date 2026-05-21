using MediatR;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Features.Sprinklers;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Features.Zones;

public class GetZonesQueryHandler : IRequestHandler<GetZonesQuery, IEnumerable<ZoneInfoDto>>
{
	private readonly SprinklerDbContext _db;
	private readonly ZoneInfoMapper _mapper;
	private readonly ISprinklerHardware _hardware;

	public GetZonesQueryHandler(SprinklerDbContext db, ZoneInfoMapper mapper, ISprinklerHardware hardware)
	{
		_db = db;
		_mapper = mapper;
		_hardware = hardware;
	}

	public async Task<IEnumerable<ZoneInfoDto>> Handle(GetZonesQuery request, CancellationToken cancellationToken)
	{
		var dbZones = (await _db.ZoneMetaDatas.AsNoTracking().ToListAsync(cancellationToken))
			.ToDictionary(z => z.ZoneId, _mapper.ModelToDto);

		return Enumerable.Range(1, _hardware.Zones)
			.Select(i => (byte)i)
			.Select(id => dbZones.TryGetValue(id, out var dto) ? dto : new ZoneInfoDto { ZoneId = id, Name = $"Zone {id}" });
	}
}
