namespace Sannel.House.Sprinklers.Core;
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
	/// <remarks>
	/// This constant is used to indicate when zone meta-data is updated.
	/// </remarks>
	public const string ZONE_META_DATA_UPDATED = "ZoneMetaDataUpdate";
}
