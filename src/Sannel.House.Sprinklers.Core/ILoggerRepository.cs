using Sannel.House.Sprinklers.Core.Models;

namespace Sannel.House.Sprinklers.Core;


/// <summary>
/// Provides an interface for logging sprinkler system station runs.
/// </summary>
public interface ILoggerRepository
{

	/// <summary>
	/// Logs the given action performed on the station with the given ID.
	/// </summary>
	/// <param name="action">The action performed on the station.</param>
	/// <param name="stationId">The ID of the station being run.</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task LogStationAction(string action, byte stationId);

	/// <summary>
	/// Logs the given action performed on the station with the given ID and the run length in milliseconds.
	/// </summary>
	/// <param name="action">The action performed on the station.</param>
	/// <param name="stationId">The ID of the station being run.</param>
	/// <param name="runLength">How long this station will run</param>
	/// <returns>A task representing the asynchronous operation.</returns>
	Task LogStationAction(string action, byte stationId, TimeSpan runLength);

	/// <summary>
	/// Gets the logs for a specified period.
	/// </summary>
	/// <param name="startDateTime">Start datetime for filter.</param>
	/// <param name="endDateTime">End datetime for filter.</param>
	/// <param name="action">The type of action to retreve</param>
	/// <returns>A task representing the asynchronous operation returning a list of StationLog instances.</returns>
	Task<IEnumerable<StationLog>> GetLogs(DateTimeOffset startDateTime, DateTimeOffset endDateTime, string action);
}
