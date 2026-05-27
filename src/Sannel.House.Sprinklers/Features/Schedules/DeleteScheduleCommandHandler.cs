using Sannel.House.Sprinklers.Features.Messaging;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class DeleteScheduleCommandHandler : IRequestHandler<DeleteScheduleCommand, bool>
{
	private readonly SprinklerDbContext _db;

	public DeleteScheduleCommandHandler(SprinklerDbContext db)
	{
		_db = db;
	}

	public async Task<bool> Handle(DeleteScheduleCommand request, CancellationToken cancellationToken)
	{
		var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
		if (program is null)
			return false;

		_db.Programs.Remove(program);
		await _db.SaveChangesAsync(cancellationToken);
		return true;
	}
}
