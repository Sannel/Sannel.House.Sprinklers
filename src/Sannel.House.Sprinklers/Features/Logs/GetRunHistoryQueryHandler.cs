using Sannel.House.Sprinklers.Features.Messaging;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Shared.Dtos.Logs;

namespace Sannel.House.Sprinklers.Features.Logs;

public class GetRunHistoryQueryHandler : IRequestHandler<GetRunHistoryQuery, IEnumerable<ZoneRunDto>>
{
	private readonly SprinklerDbContext _db;

	public GetRunHistoryQueryHandler(SprinklerDbContext db)
	{
		_db = db;
	}

	public async Task<IEnumerable<ZoneRunDto>> Handle(GetRunHistoryQuery request, CancellationToken cancellationToken)
	{
		var now = DateTimeOffset.Now;
		var start = new DateTimeOffset(request.From.ToDateTime(TimeOnly.MinValue), now.Offset);
		var end = new DateTimeOffset(request.To.ToDateTime(TimeOnly.MaxValue), now.Offset);

		return await (from l in _db.RunLog.AsNoTracking()
			where l.ActionDate >= start && l.ActionDate <= end && l.Action == LogActions.START
			join z in _db.ZoneMetaDatas on l.ZoneId equals z.ZoneId into zj
			from zone in zj.DefaultIfEmpty()
			orderby l.ActionDate descending
			select new ZoneRunDto
			{
				ActionDate = l.ActionDate,
				StationId = l.ZoneId,
				RunLength = l.RunLength,
				StationName = zone != null ? zone.Name : null,
				StationColor = zone != null ? zone.Color : null
			}).ToListAsync(cancellationToken);
	}
}
