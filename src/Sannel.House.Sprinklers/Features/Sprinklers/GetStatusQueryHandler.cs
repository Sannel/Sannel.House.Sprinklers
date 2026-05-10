using MediatR;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public class GetStatusQueryHandler : IRequestHandler<GetStatusQuery, StatusDto>
{
	private readonly SprinklerWorker _worker;
	private readonly SprinklerDbContext _db;
	private readonly ZoneInfoMapper _mapper;

	public GetStatusQueryHandler(SprinklerWorker worker, SprinklerDbContext db, ZoneInfoMapper mapper)
	{
		_worker = worker;
		_db = db;
		_mapper = mapper;
	}

	public async Task<StatusDto> Handle(GetStatusQuery request, CancellationToken cancellationToken)
	{
		var status = new StatusDto
		{
			IsRunning = _worker.IsRunning,
			TimeLeft = _worker.TimeLeft,
			TotalTime = _worker.TotalTime,
			Zones = _worker.Zones,
			QueuedZoneCount = _worker.QueuedZoneCount
		};

		if (status.IsRunning)
		{
			var zone = await _db.ZoneMetaDatas.AsNoTracking()
				.FirstOrDefaultAsync(z => z.ZoneId == _worker.StationId, cancellationToken);
			if (zone is not null)
			{
				status.ZoneInfo = _mapper.ModelToDto(zone);
			}
		}

		return status;
	}
}
