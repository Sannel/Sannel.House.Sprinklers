using System.Collections.Immutable;
using Microsoft.Extensions.Configuration;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public class FakeHardware : ISprinklerHardware
{
	private readonly bool[] _zoneStates;

	public FakeHardware(IConfiguration configuration)
	{
		var zones = configuration.GetValue<byte?>("Sprinkler:Zones")
			?? throw new InvalidOperationException("Sprinkler:Zones configuration is required");
		_zoneStates = new bool[zones];
	}

	public byte Zones => (byte)_zoneStates.Length;

	public ImmutableArray<ZoneState> State
		=> _zoneStates.Select(z => z ? ZoneState.On : ZoneState.Off).ToImmutableArray();

	public void Dispose() { }

	public Task ResetZonesAsync()
	{
		Array.Fill(_zoneStates, false);
		return Task.CompletedTask;
	}

	public Task TurnZoneOnAsync(byte zoneIndex)
	{
		if (zoneIndex < 0 || zoneIndex >= _zoneStates.Length)
		{
			throw new ArgumentOutOfRangeException(nameof(zoneIndex));
		}
		Array.Fill(_zoneStates, false);
		_zoneStates[zoneIndex] = true;
		return Task.CompletedTask;
	}
}
