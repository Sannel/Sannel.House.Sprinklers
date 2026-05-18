using System.Collections.Immutable;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public interface ISprinklerHardware : IDisposable
{
	/// <summary>
	/// Turns off all the sprinkler zones.
	/// </summary>
	Task ResetZonesAsync();

	/// <summary>
	/// Turns on the sprinkler zone.
	/// </summary>
	/// <param name="zoneIndex">The zero-based zone to turn on.</param>
	/// <exception cref="System.ArgumentOutOfRangeException">zoneIndex is greater than <see cref="Zones"/></exception>
	Task TurnZoneOnAsync(byte zoneIndex);

	/// <summary>
	/// Returns the number of zones in the system.
	/// </summary>
	byte Zones { get; }

	/// <summary>
	/// Gets the current state of all the sprinkler zones.
	/// </summary>
	ImmutableArray<ZoneState> State { get; }
}
