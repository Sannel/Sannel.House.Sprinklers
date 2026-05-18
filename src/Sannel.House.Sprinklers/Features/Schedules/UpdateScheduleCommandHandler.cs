using MediatR;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Mappers;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class UpdateScheduleCommandHandler : IRequestHandler<UpdateScheduleCommand, Guid>
{
	private readonly SprinklerDbContext _db;
	private readonly ScheduleMapper _mapper;

	public UpdateScheduleCommandHandler(SprinklerDbContext db, ScheduleMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<Guid> Handle(UpdateScheduleCommand request, CancellationToken cancellationToken)
	{
		var existing = await _db.Programs.FirstOrDefaultAsync(p => p.Id == request.Schedule.Id, cancellationToken);
		if (existing is null)
		{
			var newProgram = _mapper.DtoToModel(request.Schedule);
			await _db.Programs.AddAsync(newProgram, cancellationToken);
			await _db.SaveChangesAsync(cancellationToken);
			return newProgram.Id;
		}

		existing.Name = request.Schedule.Name;
		existing.StartTime = request.Schedule.StartTime;
		existing.DaysOfWeek = request.Schedule.DaysOfWeek;
		existing.IntervalDays = request.Schedule.IntervalDays;
		existing.IntervalStartDate = request.Schedule.IntervalStartDate;
		existing.StationTimes = request.Schedule.StationTimes
			.Select(s => new StationTime { StationId = s.StationId, RunLength = s.RunLength })
			.ToList();
		await _db.SaveChangesAsync(cancellationToken);
		return existing.Id;
	}
}
