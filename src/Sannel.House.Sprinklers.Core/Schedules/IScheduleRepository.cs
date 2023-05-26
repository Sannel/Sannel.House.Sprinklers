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
	/// <remarks>
	/// Throws System.ArgumentNullException if the program is null.
	/// </remarks>
	Task<bool> AddOrUpdateScheduleAsync(ScheduleProgram program);

	/// <summary>
	/// Gets all the schedule programs.
	/// </summary>
	/// <returns>An IEnumerable of all the schedule programs.</returns>
	Task<IEnumerable<ScheduleProgram>> GetAllSchedulesAsync();

	/// <summary>
	/// Gets all the enabled schedule programs.
	/// </summary>
	/// <returns>An IEnumerable of all the enabled schedule programs.</returns>
	Task<IEnumerable<ScheduleProgram>> GetEnabledSchedulesAsync();

	/// <summary>
	/// Get the schedule by id or return null if it's not found
	/// </summary>
	/// <param name="scheduleId">The Id of the schedule to get.</param>
	/// <returns>The found ScheduleProgram. Null if not found.</returns>
	Task<ScheduleProgram?> GetScheduleProgramAsync(Guid scheduleId);

	/// <summary>
	/// Adds a new zone run if it's not found.
	/// </summary>
	/// <param name="run">The zone run to be added.</param>
	/// <returns>A task representing the operation.</returns>
	Task AddZoneRunIfNotFoundAsync(ZoneRun run);

	/// <summary>
	/// Enables a schedule by id.
	/// </summary>
	/// <param name="scheduleId">The Id of the schedule to enable.</param>
	/// <returns>A task representing the operation.</returns>
	Task EnableScheduleAsync(Guid scheduleId);

	/// <summary>
	/// Disables a schedule by id.
	/// </summary>
	/// <param name="scheduleId">The Id of the schedule to disable.</param>
	/// <returns>A task representing the operation.</returns>
	Task DisableScheduleAsync(Guid scheduleId);

	/// <summary>
	/// Gets all the zone runs for a specific day
	/// </summary>
	/// <param name="day">The specific day to get the runs for</param>
	/// <returns>An IEnumerable of all the zone runs for the day. </returns>
	Task<IEnumerable<ZoneRun>> GetRunsByDayAsync(DateOnly day);

	/// <summary>
	/// Gets all the zone runs that have not been run for a specific day
	/// </summary>
	/// <param name="day">The specific day to get the not ran runs for</param>
	/// <returns>An IEnumerable of the not ran zone runs for the day. </returns>
	Task<IEnumerable<ZoneRun>> GetNotRanRunsByDayAsync(DateOnly day);

	/// <summary>
	/// Gets all the zone runs within a dateTime range
	/// </summary>
	/// <param name="start">The start dateTime range</param>
	/// <param name="end">The end dateTime range</param>
	/// <returns>An IEnumerable of all the zone runs that should be ran within the timespan specified. </returns>
	Task<IEnumerable<ZoneRun>> GetRunsByRangeAsync(DateOnly start, DateOnly end);

	/// <summary>
	/// Flag a run as having started at a specific dateTime and for a specific zone id
	/// </summary>
	/// <param name="startDateTime">The start dateTime of the run</param>
	/// <param name="zoneId">The zone Id of the run</param>
	/// <returns></returns>
	Task FlagRunAsStartedAsync(DateTime startDateTime, byte zoneId);
}
