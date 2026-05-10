using MediatR;
using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Features.Common;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class EnableScheduleCommandHandler : IRequestHandler<EnableScheduleCommand>
{
	private readonly SprinklerDbContext _db;

	public EnableScheduleCommandHandler(SprinklerDbContext db)
	{
		_db = db;
	}

	public async Task Handle(EnableScheduleCommand request, CancellationToken cancellationToken)
	{
		var program = await _db.Programs.FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);
		if (program is not null)
		{
			program.Enabled = request.IsEnabled;
			await _db.SaveChangesAsync(cancellationToken);
		}
	}
}
