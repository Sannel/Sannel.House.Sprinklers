using System.Collections.Immutable;
using System.Device.Gpio;
using Iot.Device.Board;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sannel.House.Sprinklers.Core.Hardware;

namespace Sannel.House.Sprinklers.Infrastructure.Hardware;

public class OpenSprinklerHardware : ISprinklerHardware
{
	private const int PIN_SR_LATCH = 22;
	private const int PIN_SR_DATA = 27;
	private const int PIN_SR_CLOCK = 4;
	private const int PIN_SR_OE = 17;

	private readonly GpioController _controller;
	private readonly ILogger<OpenSprinklerHardware> _logger;
	private readonly ZoneState[] _zones;

	/// <summary>
	/// Initializes a new instance of the <see cref="OpenSprinklerHardware"/> class.
	/// </summary>
	/// <param name="board">The board interface for the device.</param>
	/// <param name="configuration">The configuration interface for the device.</param>
	/// <param name="logger">The logger instance to log events.</param>
	public OpenSprinklerHardware(Board board, IConfiguration configuration, ILogger<OpenSprinklerHardware> logger)
	{
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this._controller = board.CreateGpioController();

		var zoneCount = configuration.GetValue<byte?>("Sprinkler:Zones") ?? throw new Exception("Sprinkler:Zones is not configured");
		this._zones = new ZoneState[zoneCount];
		this._controller.OpenPin(PIN_SR_OE, PinMode.Output);

		this._controller.OpenPin(PIN_SR_DATA, PinMode.Output);
		this._controller.OpenPin(PIN_SR_CLOCK, PinMode.Output);
		this._controller.OpenPin(PIN_SR_LATCH, PinMode.Output);
	}
	/// <summary>
	/// The number of zones to know about
	/// </summary>
	public byte Zones => (byte)this._zones.Length;

	public ImmutableArray<ZoneState> State => this._zones.ToImmutableArray();

	public void Dispose() => this._controller.Dispose();

	private void SendOutZoneInfo()
	{
		this._controller.Write(PIN_SR_OE, PinValue.High);
		this._controller.Write(PIN_SR_LATCH, PinValue.Low);

		for (var i = this._zones.Length - 1; i >= 0; i--)
		{
			this._controller.Write(PIN_SR_CLOCK, PinValue.Low);
			if (this._zones[i] == ZoneState.On)
			{
				this._controller.Write(PIN_SR_DATA, PinValue.High);
			}
			else
			{
				this._controller.Write(PIN_SR_DATA, PinValue.Low);
			}
			this._controller.Write(PIN_SR_CLOCK, PinValue.High);
		}

		this._controller.Write(PIN_SR_LATCH, PinValue.High);
		this._controller.Write(PIN_SR_OE, PinValue.Low);
		this._controller.Write(PIN_SR_LATCH, PinValue.Low);
	}

	public Task ResetZonesAsync()
		=> Task.Run(() =>
	{

		for (var i = 0; i < this._zones.Length; i++)
		{
			this._zones[i] = 0;
		}
		this.SendOutZoneInfo();
		this._logger.LogInformation("Reset all Zones");
	});

	public Task TurnZoneOnAsync(byte zoneIndex) => zoneIndex >= this.Zones || zoneIndex < 0
			? throw new ArgumentOutOfRangeException(nameof(zoneIndex), zoneIndex, "Zone index does not fall within range")
			: Task.Run(() =>
		{
			for (var i = 0; i < this._zones.Length; i++)
			{
				this._zones[i] = zoneIndex == i ? ZoneState.On : ZoneState.Off;
			}

			this.SendOutZoneInfo();
			this._logger.LogInformation("Starting Zone {zone}", zoneIndex + 1);
		});
}
