using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sannel.House.Sprinklers.Core.Schedules.Models;

namespace Sannel.House.Sprinklers.Core.Schedules;

/// <summary>
/// Interface for accessing schedule programs.
/// </summary>
public interface IScheduleRepository
{
	/// <summary>
	/// Adds or updates a schedule program.
	/// </summary>
	/// <param name="program">The schedule program to add or update.</param>
	/// <returns>A boolean value indicating whether the operation was successful.</returns>
	Task<bool> AddOrUpdateScheduleAsync(ScheduleProgram program);

	/// <summary>
	/// Gets all the schedule programs.
	/// </summary>
	/// <returns>An IEnumerable of all the schedule programs.</returns>
	Task<IEnumerable<ScheduleProgram>> GetAllSchedulesAsync();
}
