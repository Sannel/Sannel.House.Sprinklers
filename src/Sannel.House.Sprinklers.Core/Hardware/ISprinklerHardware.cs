using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Core.Hardware;

public interface ISprinklerHardware : IDisposable
{
	/// <summary>
	/// turns off all the sprinkler zones
	/// </summary>
	/// <returns></returns>
	Task ResetZonesAsync();

	/// <summary>
	/// Turns on the sprinkler zone
	/// </summary>
	/// <param name="zoneIndex">The zero based zone to turn on</param>
	/// <returns></returns>
	/// <exception cref="System.ArgumentOutOfRangeException">zoneIndex is greater then <see cref="Zones"/></exception>
	Task TurnZoneOnAsync(byte zoneIndex);

	/// <summary>
	/// Returns the number of zones in the system
	/// </summary>
	byte Zones { get; }
}
