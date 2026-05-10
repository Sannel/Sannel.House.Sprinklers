using MediatR;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Features.Zones;

public class GetZonesQueryHandler : IRequestHandler<GetZonesQuery, IEnumerable<ZoneInfoDto>>
{
	private readonly SprinklerDbContext _db;
	private readonly ZoneInfoMapper _mapper;

	public GetZonesQueryHandler(SprinklerDbContext db, ZoneInfoMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<IEnumerable<ZoneInfoDto>> Handle(GetZonesQuery request, CancellationToken cancellationToken)
		=> (await _db.ZoneMetaDatas.AsNoTracking().OrderBy(z => z.ZoneId).ToListAsync(cancellationToken))
			.Select(_mapper.ModelToDto);
}
