using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sannel.House.Sprinklers.Core.Hardware;
public class SprinklerService : BackgroundService, IDisposable
{
	private readonly ISprinklerHardware _hardware;
	private readonly ILoggerRepository _loggerRepository;
	private readonly ILogger<SprinklerService> _logger;
	private readonly bool _continue = true;
	private DateTimeOffset _endAt = DateTimeOffset.MinValue;
	private readonly PeriodicTimer _timer;
	private readonly IServiceScope _scope;

	public SprinklerService(ISprinklerHardware hardware, IServiceProvider serviceProvider, ILogger<SprinklerService> logger)
	{
		_scope = (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))).CreateScope();
		_hardware = hardware ?? throw new ArgumentNullException(nameof(hardware));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_loggerRepository = _scope.ServiceProvider.GetRequiredService<ILoggerRepository>();
		_timer = new PeriodicTimer(TimeSpan.FromMilliseconds(500));
	}

	public bool IsRunning { get; private set; } = false;

	public byte StationId { get; private set; } = 0;

	public TimeSpan TimeLeft
	{
		get
		{
			if (IsRunning)
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

	public TimeSpan? TotalTime { get; private set; }

	public byte Zones => _hardware.Zones;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (_continue && !stoppingToken.IsCancellationRequested)
		{
			await _timer.WaitForNextTickAsync(stoppingToken);
			if (IsRunning
				&& _endAt < DateTimeOffset.Now)
			{
				IsRunning = false;
				await _hardware.ResetZonesAsync();
				await _loggerRepository.LogStationAction(LogActions.FINISHED, StationId);
			}
		}

		await StopAllAsync();
	}

	public async Task<bool> StartZoneAsync(byte zoneId, TimeSpan length)
	{
		if (IsRunning || zoneId < 0 || zoneId >= _hardware.Zones || length <= TimeSpan.Zero)
		{
			return false;
		}

		TotalTime = length;
		_endAt = DateTimeOffset.Now.Add(length);
		IsRunning = true;
		await _hardware.TurnZoneOnAsync(zoneId);
		await _loggerRepository.LogStationAction(LogActions.START, zoneId, length);
		StationId = zoneId;

		return true;
	}

	public async Task<bool> StopAllAsync()
	{
		if (!IsRunning)
		{
			return false;
		}

		IsRunning = false;
		TotalTime = null;
		await _hardware.ResetZonesAsync();
		await _loggerRepository.LogStationAction(LogActions.ALL_STOP, byte.MaxValue);

		return true;
	}

	private async void loop()
	{
		while (_continue)
		{
			await _timer.WaitForNextTickAsync();
			if (IsRunning
				&& _endAt < DateTimeOffset.Now)
			{
				IsRunning = false;
				await _hardware.ResetZonesAsync();
				await _loggerRepository.LogStationAction(LogActions.FINISHED, StationId);
			}
		}
	}

	public void Dispose()
	{
		_timer.Dispose();
		_scope.Dispose();
	}

}

