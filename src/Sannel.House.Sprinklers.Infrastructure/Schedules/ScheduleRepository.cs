using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Sannel.House.Sprinklers.Core.Schedules;
using Sannel.House.Sprinklers.Core.Schedules.Models;

namespace Sannel.House.Sprinklers.Infrastructure.Schedules;

/// <summary>
/// The class responsible for CRUD operations on the schedule table.
/// </summary>
public class ScheduleRepository : IScheduleRepository
{
	private readonly SprinklerDbContext _context;
	private readonly ILogger _logger;

	/// <summary>
	/// Constructor for ScheduleRepository class.
	/// </summary>
	/// <param name="context">The ScheduleDbContext.</param>
	/// <param name="logger">The ILogger.</param>
	public ScheduleRepository(SprinklerDbContext context, ILogger<ScheduleRepository> logger)
	{
		_context = context ?? throw new ArgumentNullException(nameof(context));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	/// <summary>
	/// Adds or updates schedule program in ScheduleDbContext.
	/// </summary>
	/// <param name="program">The schedule program.</param>
	/// <returns>True if schedule was added, false if it was updated.</returns>
	public async Task<bool> AddOrUpdateScheduleAsync(ScheduleProgram program)
	{
		if (program == null)
		{
			throw new ArgumentNullException(nameof(program));
		}

		var existingProgram = await _context.Programs.FirstOrDefaultAsync(p => p.Id == program.Id);

		if (existingProgram is not null)
		{
			existingProgram.Name = program.Name;
			existingProgram.Enabled = program.Enabled;
			existingProgram.ScheduleCron = program.ScheduleCron;
			existingProgram.StationTimes = program.StationTimes;

			_context.Update(existingProgram);
			await _context.SaveChangesAsync();

			return false;
		}
		else
		{
			await _context.Programs.AddAsync(program);
			await _context.SaveChangesAsync();

			return true;
		}
	}

	/// <summary>
	/// Adds ZoneRun to the db if it doesn't exist already.
	/// </summary>
	/// <param name="run">The run object to add.</param>
	/// <returns>A Task.</returns>
	public async Task AddZoneRunIfNotFoundAsync(ZoneRun run)
	{
		if (!_context.Runs.Where(i => i.ZoneId == run.ZoneId && i.StartDateTime == run.StartDateTime).Any())
		{
			await _context.Runs.AddAsync(run);
			await _context.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Sets the enabled value of ScheduleProgram to false.
	/// </summary>
	/// <param name="scheduleId">The ID of the schedule to disable.</param>
	public async Task DisableScheduleAsync(Guid scheduleId)
	{
		var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == scheduleId);

		if (program is not null)
		{
			program.Enabled = false;
			await _context.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Sets the enabled value of ScheduleProgram to true.
	/// </summary>
	/// <param name="scheduleId">The ID of the schedule to enable.</param>
	public async Task EnableScheduleAsync(Guid scheduleId)
	{
		var program = await _context.Programs.FirstOrDefaultAsync(p => p.Id == scheduleId);

		if (program is not null)
		{
			program.Enabled = true;
			await _context.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Flags a run as started.
	/// </summary>
	/// <param name="startDateTime">The date/time the run was started.</param>
	/// <param name="zoneId">The zone ID of the run.</param>
	public async Task FlagRunAsStartedAsync(DateTime startDateTime, byte zoneId)
	{
		var first = await _context.Runs
			.FirstOrDefaultAsync(i => i.StartDateTime == startDateTime
									&& i.ZoneId == zoneId);

		if(first is not null)
		{
			first.Ran = true;
			first.StartedAt = DateTime.Now;
			await _context.SaveChangesAsync();
		}
	}

	/// <summary>
	/// Gets all schedules from ScheduleDbContext.
	/// </summary>
	/// <returns>An IEnumerable of ScheduleProgram objects.</returns>
	public async Task<IEnumerable<ScheduleProgram>> GetAllSchedulesAsync()
		=> await _context.Programs.AsNoTracking().OrderBy(i => i.Name).ToArrayAsync();

	/// <summary>
	/// Retrieves all enabled schedules from the ScheduleDbContext.
	/// </summary>
	/// <returns>An IEnumerable of ScheduleProgram objects.</returns>
	public async Task<IEnumerable<ScheduleProgram>> GetEnabledSchedulesAsync()
	{
		return await _context.Programs.Where(i => i.Enabled).AsNoTracking().ToArrayAsync();
	}

	private static readonly Func<DbContext, DateTime, DateTime, IAsyncEnumerable<ZoneRun>> _zoneNotRunByTimeRange
		= EF.CompileAsyncQuery((DbContext dbContext, DateTime start, DateTime end) =>
			dbContext.Set<ZoneRun>()
			.Where(i => i.StartDateTime >= start
						&& i.StartDateTime <= end
						&& i.Ran == false)
			.OrderBy(i => i.StartDateTime)
			.AsNoTracking());

	/// <summary>
	/// Retrieves zone runs that have not yet run on a specified day.
	/// </summary>
	/// <param name="day">The specified day.</param>
	/// <returns>An IEnumerable of ZoneRun objects.</returns>
	public async Task<IEnumerable<ZoneRun>> GetNotRanRunsByDayAsync(DateOnly day)
	{
		var result = _zoneNotRunByTimeRange(_context,
						day.ToDateTime(TimeOnly.MinValue),
						day.ToDateTime(TimeOnly.MaxValue));
		var list = new List<ZoneRun>();
		await foreach(var zr in result)
		{
			list.Add(zr);
		}

		return list;
	}

	private static readonly Func<DbContext, DateTime, DateTime, IAsyncEnumerable<ZoneRun>> _zoneRunByTimeRange
		= EF.CompileAsyncQuery((DbContext dbContext, DateTime start, DateTime end) =>
			dbContext.Set<ZoneRun>()
			.Where(i => i.StartDateTime >= start
						&& i.StartDateTime <= end)
			.OrderBy(i => i.StartDateTime)
			.AsNoTracking());

	/// <summary>
	/// Retrieves all zone runs for a specified day.
	/// </summary>
	/// <param name="day">The specified day.</param>
	/// <returns>An IEnumerable of ZoneRun objects.</returns>
	public async Task<IEnumerable<ZoneRun>> GetRunsByDayAsync(DateOnly day)
	{
		var result = _zoneRunByTimeRange(_context,
						day.ToDateTime(TimeOnly.MinValue),
						day.ToDateTime(TimeOnly.MaxValue));
		var list = new List<ZoneRun>();
		await foreach(var zr in result)
		{
			list.Add(zr);
		}

		return list;
	}

	/// <summary>
	/// Retrieves all zone runs for a specified range of days.
	/// </summary>
	/// <param name="start">The start day.</param>
	/// <param name="end">The end day.</param>
	/// <returns>An IEnumerable of ZoneRun objects.</returns>
	public async Task<IEnumerable<ZoneRun>> GetRunsByRangeAsync(DateOnly start, DateOnly end)
	{
		var result = _zoneRunByTimeRange(_context,
						start.ToDateTime(TimeOnly.MinValue),
						end.ToDateTime(TimeOnly.MaxValue));
		var list = new List<ZoneRun>();
		await foreach(var zr in result)
		{
			list.Add(zr);
		}

		return list;
	}

	/// <summary>
	/// Gets a ScheduleProgram with a specified ID.
	/// </summary>
	/// <param name="scheduleId">The specified ID.</param>
	/// <returns>A ScheduleProgram object.</returns>
	public Task<ScheduleProgram?> GetScheduleProgramAsync(Guid scheduleId)
		=> _context.Programs.AsNoTracking().FirstOrDefaultAsync(i => i.Id == scheduleId);
}
