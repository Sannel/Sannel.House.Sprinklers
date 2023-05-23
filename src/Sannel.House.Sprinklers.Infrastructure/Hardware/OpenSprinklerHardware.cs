using System;
using System.Collections.Generic;
using System.Device.Gpio;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Iot.Device.Board;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sannel.House.Sprinklers.Core.Hardware;

namespace Sannel.House.Sprinklers.Infrastructure.Hardware;

public class OpenSprinklerHardware : ISprinklerHardware
{
	const int PIN_SR_LATCH = 22;
	const int PIN_SR_DATA = 27;
	const int PIN_SR_CLOCK = 4;
	const int PIN_SR_OE = 17;

	private readonly Board _board;
	private readonly GpioController _controller;
	private readonly IConfiguration _configuration;
	private readonly ILogger<OpenSprinklerHardware> _logger;
	private readonly byte[] _zones;

	/// <summary>
	/// Initializes a new instance of the <see cref="OpenSprinklerHardware"/> class.
	/// </summary>
	/// <param name="board">The board interface for the device.</param>
	/// <param name="configuration">The configuration interface for the device.</param>
	/// <param name="logger">The logger instance to log events.</param>
	public OpenSprinklerHardware(Board board, IConfiguration configuration, ILogger<OpenSprinklerHardware> logger)
	{
		_board = board ?? throw new ArgumentNullException(nameof(board));
		_configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_controller = board.CreateGpioController();

		var zoneCount = configuration.GetValue<byte?>("Sprinkler:Zones") ?? throw new Exception("Sprinkler:Zones is not configured");
		_zones = new byte[zoneCount];
		_controller.OpenPin(PIN_SR_OE, PinMode.Output);

		_controller.OpenPin(PIN_SR_DATA, PinMode.Output);
		_controller.OpenPin(PIN_SR_CLOCK, PinMode.Output);
		_controller.OpenPin(PIN_SR_LATCH, PinMode.Output);
	}
	/// <summary>
	/// The number of zones to know about
	/// </summary>
	public byte Zones => (byte)_zones.Length;

	public void Dispose()
	{
		_controller.Dispose();
	}

	private void SendOutZoneInfo()
	{
		_controller.Write(PIN_SR_OE, PinValue.High);
		_controller.Write(PIN_SR_LATCH, PinValue.Low);

		var builder = new StringBuilder();

		for(var i=_zones.Length - 1;i >=0;i--)
		{
			_controller.Write(PIN_SR_CLOCK, PinValue.Low);
			if (_zones[i] >= 1)
			{
				builder.Append('1');
				_controller.Write(PIN_SR_DATA, PinValue.High);
			}
			else
			{
				builder.Append('0');
				_controller.Write(PIN_SR_DATA, PinValue.Low);
			}
			_controller.Write(PIN_SR_CLOCK, PinValue.High);
		}

		_logger.LogInformation("Sent out {b}", builder);

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
	});

	public Task TurnZoneOnAsync(byte zoneIndex)
	{
		if(zoneIndex >= Zones || zoneIndex < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(zoneIndex), zoneIndex, "Zone index does not fall within range");
		}

		return Task.Run(() =>
		{
			for (var i = 0; i < _zones.Length; i++)
			{
				if (zoneIndex == i)
				{
					_zones[i] = 1;
				}
				else
				{
					_zones[i] = 0;
				}
			}

			SendOutZoneInfo();
			_logger.LogInformation("Starting Zone {zone}", zoneIndex + 1);
		});
	}
}
