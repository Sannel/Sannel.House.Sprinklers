using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sannel.House.Sprinklers.Core.Hardware;
public class SprinklerService : BackgroundService, IDisposable
{
	private readonly ISprinklerHardware _hardware;
	private readonly ILoggerRepository _loggerRepository;
	private readonly ILogger<SprinklerService> _logger;
	private bool _isRunning = false;
	private bool _continue = true;
	private DateTimeOffset _endAt = DateTimeOffset.MinValue;
	private byte _stationId = 0;
	private readonly PeriodicTimer _timer;

	public SprinklerService(ISprinklerHardware hardware, ILoggerRepository loggerRepository, ILogger<SprinklerService> logger)
	{
		this._hardware = hardware ?? throw new ArgumentNullException(nameof(hardware));
		this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
		this._loggerRepository = loggerRepository ?? throw new ArgumentNullException(nameof(loggerRepository));
		_timer = new PeriodicTimer(TimeSpan.FromMilliseconds(100));
	}

	public bool IsRunning => _isRunning;

	public byte StationId => _stationId;

	public TimeSpan TimeLeft
	{
		get
		{
			if (_isRunning)
			{
				var c = new TimeSpan(DateTimeOffset.Now.Ticks);
				var end = new TimeSpan(_endAt.Ticks);

				return end - c;
			}
			else
			{
				return TimeSpan.Zero;
			}
		}
	}

	public byte Zones => _hardware.Zones;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (_continue && !stoppingToken.IsCancellationRequested)
		{
			await _timer.WaitForNextTickAsync(stoppingToken);
			if (_isRunning
				&& _endAt < DateTimeOffset.Now)
			{
				_isRunning = false;
				await _hardware.ResetZonesAsync();
				await _loggerRepository.LogStationAction("Finish", StationId);
			}
		}

		await StopAllAsync();
	}

	public async Task<bool> StartZoneAsync(byte zoneId, TimeSpan length)
	{
		if (_isRunning || zoneId < 0 || zoneId >= _hardware.Zones || length <= TimeSpan.Zero)
		{
			return false;
		}

		_endAt = DateTimeOffset.Now.Add(length);
		_isRunning = true;
		await _hardware.TurnZoneOnAsync(zoneId);
		await _loggerRepository.LogStationAction("Start", zoneId, length);
		_stationId = zoneId;

		return true;
	}

	public async Task<bool> StopAllAsync()
	{
		if (!_isRunning)
		{
			return false;
		}

		_isRunning = false;
		await _hardware.ResetZonesAsync();
		await _loggerRepository.LogStationAction("AllStop", byte.MaxValue);

		return true;
	}

	private async void loop()
	{
		while (_continue)
		{
			await _timer.WaitForNextTickAsync();
			if (_isRunning
				&& _endAt < DateTimeOffset.Now)
			{
				_isRunning = false;
				await _hardware.ResetZonesAsync();
				await _loggerRepository.LogStationAction("Finish", StationId);
			}
		}
	}

	public void Dispose()
	{
		_timer.Dispose();
	}

}

