using Sannel.House.Sprinklers.Features.Messaging;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Mappers;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class CreateScheduleCommandHandler : IRequestHandler<CreateScheduleCommand, Guid>
{
	private readonly SprinklerDbContext _db;
	private readonly ScheduleMapper _mapper;

	public CreateScheduleCommandHandler(SprinklerDbContext db, ScheduleMapper mapper)
	{
		_db = db;
		_mapper = mapper;
	}

	public async Task<Guid> Handle(CreateScheduleCommand request, CancellationToken cancellationToken)
	{
		var program = _mapper.DtoToModel(request.Schedule);
		program.Id = Guid.NewGuid();
		await _db.Programs.AddAsync(program, cancellationToken);
		await _db.SaveChangesAsync(cancellationToken);
		return program.Id;
	}
}
