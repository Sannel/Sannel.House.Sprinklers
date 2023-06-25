using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Core.Hardware;
public class SprinklerService : BackgroundService, IDisposable
{
	private readonly ISprinklerHardware _hardware;
	private readonly ILoggerRepository _loggerRepository;
	private readonly ILogger<SprinklerService> _logger;
	private readonly bool _continue = true;
	private readonly IMessageClient _messageClient;
	private DateTimeOffset _endAt = DateTimeOffset.MinValue;
	private readonly TimeSpan _waitTime;
	private readonly IServiceScope _scope;

	public SprinklerService(ISprinklerHardware hardware,
		IServiceProvider serviceProvider,
		IMessageClient messageClient,
		ILogger<SprinklerService> logger)
	{
		_scope = (serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider))).CreateScope();
		_hardware = hardware ?? throw new ArgumentNullException(nameof(hardware));
		_messageClient = messageClient ?? throw new ArgumentNullException(nameof(messageClient));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
		_loggerRepository = _scope.ServiceProvider.GetRequiredService<ILoggerRepository>();
		_waitTime = TimeSpan.FromMilliseconds(500);
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

	private async void sendStopMessage(StationStopMessage message)
	{
		await _messageClient.SendStopMessageAsync(message);
	}

	private async void sendProgressMessage(StationProgressMessage message)
	{
		await _messageClient.SendProgressMessageAsync(message);
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (_continue && !stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(_waitTime, stoppingToken);
			if (IsRunning
				&& _endAt < DateTimeOffset.Now)
			{
				var totalTime = TotalTime ?? TimeSpan.FromMilliseconds(1);
				sendProgressMessage(new StationProgressMessage()
				{
					ZoneId = StationId,
					RunLength = totalTime,
					TimeLeft = totalTime,
					PercentCompleteFloat = 1f,
					PercentComplete = 100,
					InvertPercentComplete = 0,
					TriggerTime = DateTimeOffset.Now
				});
				sendStopMessage(new StationStopMessage
				{
					ZoneId = StationId,
					StopTime = DateTimeOffset.Now,
					TriggerTime = DateTimeOffset.Now
				});
				IsRunning = false;
				await _hardware.ResetZonesAsync();
				await _loggerRepository.LogStationAction(LogActions.FINISHED, StationId);
			}
			else if(IsRunning)
			{
				try
				{
					var totalTime = TotalTime ?? TimeSpan.FromMilliseconds(1);
					var timeLeft = TimeLeft;
					var percent = (float)(1f - (timeLeft.TotalMicroseconds / totalTime.TotalMicroseconds));
					sendProgressMessage(new StationProgressMessage
					{
						ZoneId = StationId,
						RunLength = totalTime,
						TimeLeft = timeLeft,
						PercentCompleteFloat = percent,
						PercentComplete = (short)(percent * 100),
						InvertPercentComplete = (short)(100 - (percent * 100)),
						TriggerTime = DateTimeOffset.Now
					});
				}
				catch (Exception ex)
				{

				}
			}
		}

		await StopAllAsync();
	}

	private async void sendStartMessage(StationStartMessage message)
	{
		await _messageClient.SendStartMessageAsync(message);
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
		sendStartMessage(new StationStartMessage
		{
			ZoneId = zoneId,
			Duration = length,
			StartTime = DateTimeOffset.Now,
			TriggerTime = DateTimeOffset.Now
		});
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
			await Task.Delay(_waitTime);
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
		_scope.Dispose();
	}

}

