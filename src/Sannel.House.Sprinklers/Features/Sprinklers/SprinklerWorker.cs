using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Features.Logs;
using Sannel.House.Sprinklers.Features.Notifications;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public class SprinklerWorker : BackgroundService
{
	private readonly ISprinklerHardware _hardware;
	private readonly IServiceProvider _provider;
	private readonly ILogger<SprinklerWorker> _logger;
	private readonly Queue<(byte ZoneId, TimeSpan Length)> _queue = new();
	private readonly SemaphoreSlim _queueLock = new(1, 1);

	private DateTimeOffset _endAt = DateTimeOffset.MinValue;
	private byte _stationId;
	private TimeSpan _totalTime;
	private bool _wasRunning;

	public bool IsRunning => _endAt > DateTimeOffset.Now;
	public byte StationId => _stationId;
	public TimeSpan? TimeLeft => IsRunning ? _endAt - DateTimeOffset.Now : null;
	public TimeSpan TotalTime => _totalTime;
	public byte Zones => _hardware.Zones;
	public int QueuedZoneCount => _queue.Count;

	public SprinklerWorker(ISprinklerHardware hardware, IServiceProvider provider, ILogger<SprinklerWorker> logger)
	{
		_hardware = hardware;
		_provider = provider;
		_logger = logger;
	}

	public async Task EnqueueZonesAsync(IEnumerable<ZoneStartRequestDto> zones)
	{
		await _queueLock.WaitAsync();
		try
		{
			foreach (var zone in zones)
				_queue.Enqueue((zone.ZoneId, zone.Length));
		}
		finally
		{
			_queueLock.Release();
		}
	}

	public async Task StopAllAsync()
	{
		await _queueLock.WaitAsync();
		try
		{
			_queue.Clear();
			if (_wasRunning)
			{
				_endAt = DateTimeOffset.MinValue;
				_wasRunning = false;
				await _hardware.ResetZonesAsync();
				using var scope = _provider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
				var messageClient = scope.ServiceProvider.GetRequiredService<IMessageClient>();
				await db.RunLog.AddAsync(new StationLog
				{
					Id = Guid.NewGuid(),
					Action = LogActions.ALL_STOP,
					ActionDate = DateTimeOffset.Now,
					ZoneId = _stationId
				});
				await db.SaveChangesAsync();
				await messageClient.SendStopMessageAsync(new StationStopMessage
				{
					ZoneId = _stationId,
					StopTime = DateTimeOffset.Now,
					TriggerTime = DateTimeOffset.Now
				});
			}
		}
		finally
		{
			_queueLock.Release();
		}
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				await _queueLock.WaitAsync(stoppingToken);
				try
				{
					var isCurrentlyRunning = IsRunning;

					if (isCurrentlyRunning)
					{
						_wasRunning = true;
						var timeLeft = TimeLeft!.Value;
						var totalTime = _totalTime;
						var percent = totalTime.TotalMilliseconds > 0
							? (float)(1f - (timeLeft.TotalMilliseconds / totalTime.TotalMilliseconds))
							: 1f;

						using var scope = _provider.CreateScope();
						var messageClient = scope.ServiceProvider.GetRequiredService<IMessageClient>();
						await messageClient.SendProgressMessageAsync(new StationProgressMessage
						{
							ZoneId = _stationId,
							RunLength = totalTime,
							TimeLeft = timeLeft,
							PercentCompleteFloat = percent,
							PercentComplete = (short)(percent * 100),
							InvertPercentComplete = (short)(100 - (percent * 100)),
							TriggerTime = DateTimeOffset.Now
						});
					}
					else if (_wasRunning)
					{
						_wasRunning = false;
						var finishedZone = _stationId;
						var finishedLength = _totalTime;
						await _hardware.ResetZonesAsync();

						using var scope = _provider.CreateScope();
						var db = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
						var messageClient = scope.ServiceProvider.GetRequiredService<IMessageClient>();
						await db.RunLog.AddAsync(new StationLog
						{
							Id = Guid.NewGuid(),
							Action = LogActions.FINISHED,
							ActionDate = DateTimeOffset.Now,
							ZoneId = finishedZone,
							RunLength = finishedLength
						});
						await db.SaveChangesAsync();
						await messageClient.SendStopMessageAsync(new StationStopMessage
						{
							ZoneId = finishedZone,
							StopTime = DateTimeOffset.Now,
							TriggerTime = DateTimeOffset.Now
						});

						if (_queue.TryDequeue(out var next))
						{
							await StartZoneInternalAsync(next, stoppingToken);
						}
					}
					else if (_queue.TryDequeue(out var next))
					{
						await StartZoneInternalAsync(next, stoppingToken);
					}
				}
				finally
				{
					_queueLock.Release();
				}
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in SprinklerWorker");
			}

			await Task.Delay(500, stoppingToken);
		}

		await _hardware.ResetZonesAsync();
	}

	private async Task StartZoneInternalAsync((byte ZoneId, TimeSpan Length) zone, CancellationToken cancellationToken)
	{
		_stationId = zone.ZoneId;
		_totalTime = zone.Length;
		_endAt = DateTimeOffset.Now.Add(zone.Length);
		_wasRunning = true;

		await _hardware.TurnZoneOnAsync((byte)(_stationId - 1));

		using var scope = _provider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
		var messageClient = scope.ServiceProvider.GetRequiredService<IMessageClient>();

		await db.RunLog.AddAsync(new StationLog
		{
			Id = Guid.NewGuid(),
			Action = LogActions.START,
			ActionDate = DateTimeOffset.Now,
			ZoneId = _stationId,
			RunLength = _totalTime
		});
		await db.SaveChangesAsync(cancellationToken);

		await messageClient.SendStartMessageAsync(new StationStartMessage
		{
			ZoneId = _stationId,
			Duration = _totalTime,
			StartTime = DateTimeOffset.Now,
			TriggerTime = DateTimeOffset.Now
		});

		_logger.LogInformation("Started zone {ZoneId} for {Length}", _stationId, _totalTime);
	}
}
