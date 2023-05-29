using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sannel.House.Sprinklers.Core.Hardware;

/// <summary>
/// Enum representing the state of a sprinkler zone.
/// </summary>
public enum ZoneState : byte
{
	/// <summary>
	/// Sprinkler zone is off.
	/// </summary>
	Off = 0,

	/// <summary>
	/// Sprinkler zone is on.
	/// </summary>
	On = 1
}
