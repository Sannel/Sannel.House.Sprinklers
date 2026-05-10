using MediatR;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Features.Zones;

public class GetZoneByIdQueryHandler : IRequestHandler<GetZoneByIdQuery, ZoneInfoDto?>
{
	private readonly SprinklerDbContext _db;
	private readonly ZoneInfoMapper _mapper;

	public GetZoneByIdQueryHandler(SprinklerDbContext db, ZoneInfoMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<ZoneInfoDto?> Handle(GetZoneByIdQuery request, CancellationToken cancellationToken)
	{
		var zone = await _db.ZoneMetaDatas.AsNoTracking()
			.FirstOrDefaultAsync(z => z.ZoneId == request.Id, cancellationToken);
		return zone is not null ? _mapper.ModelToDto(zone) : null;
	}
}
