using Sannel.House.Sprinklers.Features.Messaging;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class GetSchedulesQueryHandler : IRequestHandler<GetSchedulesQuery, IEnumerable<ScheduleProgramDto>>
{
	private readonly SprinklerDbContext _db;
	private readonly ScheduleMapper _mapper;

	public GetSchedulesQueryHandler(SprinklerDbContext db, ScheduleMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<IEnumerable<ScheduleProgramDto>> Handle(GetSchedulesQuery request, CancellationToken cancellationToken)
		=> (await _db.Programs.AsNoTracking().OrderBy(p => p.Name).ToListAsync(cancellationToken))
			.Select(_mapper.ModelToDto);
}
