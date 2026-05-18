namespace Sannel.House.Sprinklers.Features.Logs;

/// <summary>
/// Defines constants for logging actions.
/// </summary>
public static class LogActions
{
	/// <summary>
	/// Constant representing the start of an action.
	/// </summary>
	public const string START = "Start";

	/// <summary>
	/// Constant representing the finish of an action.
	/// </summary>
	public const string FINISHED = "Finished";

	/// <summary>
	/// Constant representing stopping of all Sprinklers.
	/// </summary>
	public const string ALL_STOP = "AllStop";

	/// <summary>
	/// Constant representing the update of zone meta-data.
	/// </summary>
	public const string ZONE_META_DATA_UPDATED = "ZoneMetaDataUpdate";
}
