using System.Collections.Immutable;
using Sannel.House.Sprinklers.Core.Hardware;

namespace Sannel.House.Sprinklers.Infrastructure.Hardware;

public class FakeHardware : ISprinklerHardware
{
	private readonly ZoneState[] _zones = new ZoneState[8];
	public byte Zones => (byte)this._zones.Length;

	public void Dispose()
	{
	}

	public Task ResetZonesAsync()
	{
		Array.Fill(this._zones, ZoneState.Off);
		return Task.CompletedTask;
	}

	public Task TurnZoneOnAsync(byte zoneIndex)
	{
		if (zoneIndex < 0 || zoneIndex >= this._zones.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(zoneIndex));
		}
		this._zones[zoneIndex] = ZoneState.On;

		return Task.CompletedTask;
	}
	public ImmutableArray<ZoneState> State => this._zones.ToImmutableArray();
}
