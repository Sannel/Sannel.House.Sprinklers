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
		var logs = await (from l in _context.RunLog.AsNoTracking()
			where l.ActionDate >= startDateTime
			&& l.ActionDate <= endDateTime
			&& l.Action == action
			join s in _context.ZoneMetaDatas 
				on l.ZoneId equals s.ZoneId
			orderby l.ActionDate descending
			select new StationLog()
			{
				ZoneId = l.ZoneId,
				Action = l.Action,
				ActionDate = l.ActionDate,
				Id = l.Id,
				RunLength = l.RunLength,
				UserId = l.UserId,
				Username = l.Username,
				StationColor = s.Color,
				StationName = s.Name
			}).ToListAsync();

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
