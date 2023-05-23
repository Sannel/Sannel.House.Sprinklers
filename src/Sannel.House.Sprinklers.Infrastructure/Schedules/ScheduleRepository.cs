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
/// Class responsible for managing schedules in the ScheduleDbContext.
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
	public ScheduleRepository(SprinklerDbContext context, ILogger logger)
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
	/// Gets all schedules from ScheduleDbContext.
	/// </summary>
	/// <returns>An IEnumerable of ScheduleProgram objects.</returns>
	public async Task<IEnumerable<ScheduleProgram>> GetAllSchedulesAsync()
		=> await _context.Programs.OrderBy(i => i.Name).ToArrayAsync();
}
