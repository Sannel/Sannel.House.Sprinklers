using MediatR;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class GetScheduleByIdQueryHandler : IRequestHandler<GetScheduleByIdQuery, ScheduleProgramDto?>
{
	private readonly SprinklerDbContext _db;
	private readonly ScheduleMapper _mapper;

	public GetScheduleByIdQueryHandler(SprinklerDbContext db, ScheduleMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<ScheduleProgramDto?> Handle(GetScheduleByIdQuery request, CancellationToken cancellationToken)
	{
		var p = await _db.Programs.AsNoTracking().FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
		return p is not null ? _mapper.ModelToDto(p) : null;
	}
}
