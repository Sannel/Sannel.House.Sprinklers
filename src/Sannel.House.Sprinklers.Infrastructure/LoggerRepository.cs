using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Core.Models;

namespace Sannel.House.Sprinklers.Infrastructure;

public class LoggerRepository : ILoggerRepository
{
	private readonly SprinklerDbContext context;

	public LoggerRepository(SprinklerDbContext context)
	{
		this.context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task LogStationAction(string action, byte stationId)
	{
		ArgumentNullException.ThrowIfNullOrEmpty(nameof(action), action);
		var log = new StationLog()
		{
			Id = Guid.NewGuid(),
			Action = action,
			ActionDate = DateTimeOffset.Now,
			StationId = stationId
		};
		await context.RunLog.AddAsync(log);
		await context.SaveChangesAsync();
	}

	public async Task LogStationAction(string action, byte stationId, TimeSpan runLength)
	{
		ArgumentNullException.ThrowIfNullOrEmpty(nameof(action), action);
		var log = new StationLog()
		{
			Id = Guid.NewGuid(),
			Action = action,
			ActionDate = DateTimeOffset.Now,
			RunLength = runLength,
			StationId = stationId
		};
		await context.RunLog.AddAsync(log);
		await context.SaveChangesAsync();
	}
}
