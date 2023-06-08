using Microsoft.EntityFrameworkCore;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Core.Models;

namespace Sannel.House.Sprinklers.Infrastructure;

public class LoggerRepository : ILoggerRepository
{
	private readonly SprinklerDbContext _context;

	public LoggerRepository(SprinklerDbContext context)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
	}

	public async Task<IEnumerable<StationLog>> GetLogs(DateTimeOffset startDateTime, DateTimeOffset endDateTime, string action)
	{
		var logs = await _context.RunLog.AsNoTracking().Where(i =>
							i.ActionDate >= startDateTime
							&& i.ActionDate <= endDateTime
							&& i.Action == action)
						.OrderByDescending(i => i.ActionDate)
						.ToListAsync();

		return logs;
	}

	public async Task LogStationAction(string action, byte stationId)
	{
		ArgumentNullException.ThrowIfNullOrEmpty(nameof(action), action);
		var log = new StationLog()
		{
			Id = Guid.NewGuid(),
			Action = action,
			ActionDate = DateTimeOffset.Now,
			ZoneId = stationId
		};
		await _context.RunLog.AddAsync(log);
		await _context.SaveChangesAsync();
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
			ZoneId = stationId
		};
		await _context.RunLog.AddAsync(log);
		await _context.SaveChangesAsync();
	}
}
