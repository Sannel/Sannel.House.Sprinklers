using System.Collections.Immutable;
using System.Device.Gpio;
using Iot.Device.Board;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public class OpenSprinklerHardware : ISprinklerHardware
{
	private const int PIN_SR_LATCH = 22;
	private const int PIN_SR_DATA = 27;
	private const int PIN_SR_CLOCK = 4;
	private const int PIN_SR_OE = 17;

	private readonly GpioController _controller;
	private readonly ILogger<OpenSprinklerHardware> _logger;
	private readonly ZoneState[] _zones;

	public OpenSprinklerHardware(Board board, IConfiguration configuration, ILogger<OpenSprinklerHardware> logger)
	{
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_controller = board.CreateGpioController();

		var zoneCount = configuration.GetValue<byte?>("Sprinkler:Zones") ?? throw new Exception("Sprinkler:Zones is not configured");
		_zones = new ZoneState[zoneCount];
		_controller.OpenPin(PIN_SR_OE, PinMode.Output);
		_controller.OpenPin(PIN_SR_DATA, PinMode.Output);
		_controller.OpenPin(PIN_SR_CLOCK, PinMode.Output);
		_controller.OpenPin(PIN_SR_LATCH, PinMode.Output);
	}

	public byte Zones => (byte)_zones.Length;

	public ImmutableArray<ZoneState> State => _zones.ToImmutableArray();

	public void Dispose() => _controller.Dispose();

	private void SendOutZoneInfo()
	{
		_controller.Write(PIN_SR_OE, PinValue.High);
		_controller.Write(PIN_SR_LATCH, PinValue.Low);

		for (var i = _zones.Length - 1; i >= 0; i--)
		{
			_controller.Write(PIN_SR_CLOCK, PinValue.Low);
			_controller.Write(PIN_SR_DATA, _zones[i] == ZoneState.On ? PinValue.High : PinValue.Low);
			_controller.Write(PIN_SR_CLOCK, PinValue.High);
		}

		_controller.Write(PIN_SR_LATCH, PinValue.High);
		_controller.Write(PIN_SR_OE, PinValue.Low);
		_controller.Write(PIN_SR_LATCH, PinValue.Low);
	}

	public Task ResetZonesAsync()
		=> Task.Run(() =>
		{
			for (var i = 0; i < _zones.Length; i++)
			{
				_zones[i] = 0;
			}
			SendOutZoneInfo();
			_logger.LogInformation("Reset all Zones");
		});

	public Task TurnZoneOnAsync(byte zoneIndex) => zoneIndex >= Zones || zoneIndex < 0
		? throw new ArgumentOutOfRangeException(nameof(zoneIndex), zoneIndex, "Zone index does not fall within range")
		: Task.Run(() =>
		{
			for (var i = 0; i < _zones.Length; i++)
			{
				_zones[i] = zoneIndex == i ? ZoneState.On : ZoneState.Off;
			}
			SendOutZoneInfo();
			_logger.LogInformation("Starting Zone {zone}", zoneIndex + 1);
		});
}
