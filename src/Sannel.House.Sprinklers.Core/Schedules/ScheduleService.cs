using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sannel.House.Sprinklers.Core.Hardware;
using Sannel.House.Sprinklers.Core.Schedules.Models;

namespace Sannel.House.Sprinklers.Core.Schedules;

public class ScheduleService : BackgroundService
{
	private readonly IScheduleRepository _scheduleRepository;
	private readonly PeriodicTimer _waitStart;
	private readonly ILogger<ScheduleService> _logger;
	private readonly SprinklerService _sprinklers;
	private readonly IServiceScope _scope;

	public ScheduleService(IServiceProvider provider)
	{
		ArgumentNullException.ThrowIfNull(provider);
		_scope = provider.CreateScope();
		_scheduleRepository = _scope.ServiceProvider.GetRequiredService<IScheduleRepository>();
		_sprinklers = _scope.ServiceProvider.GetRequiredService<SprinklerService>();
		_logger = _scope.ServiceProvider.GetRequiredService<ILogger<ScheduleService>>();
		_waitStart = new PeriodicTimer(TimeSpan.FromSeconds(10));
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		try
		{
			GenerateSchedule(stoppingToken);
			while (!stoppingToken.IsCancellationRequested)
			{
				var needRun = await _scheduleRepository.GetNotRanRunsByDayAsync(DateOnly.FromDateTime(DateTime.Now));
				var first = needRun.FirstOrDefault();

				if (first?.StartDateTime < DateTime.Now)
				{
					if (!_sprinklers.IsRunning)
					{
						await _sprinklers.StartZoneAsync(first.ZoneId, first.RunLength);
						await _scheduleRepository.FlagRunAsStartedAsync(first.StartDateTime, first.ZoneId);
					}
				}

				try
				{
					await _waitStart.WaitForNextTickAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Exception in {service}", nameof(ScheduleService));
				}
			}
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Exception in {service}", nameof(ScheduleService));
			throw;
		}
	}

	private async void GenerateSchedule(CancellationToken stoppingToken)
	{
		while (true && !stoppingToken.IsCancellationRequested)
		{
			_logger.LogInformation("Generating Schedule");
			var endTime = DateTime.MinValue;
			var date = DateOnly.FromDateTime(DateTime.Now);
			var schedules = await _scheduleRepository.GetEnabledSchedulesAsync();
			foreach (var schedule in schedules)
			{
				_logger.LogDebug("Schedule Cron {Cron}", schedule.ScheduleCron);
				var nschedule = NCrontab.CrontabSchedule.TryParse(schedule.ScheduleCron,
					new NCrontab.CrontabSchedule.ParseOptions()
					{
						IncludingSeconds = false
					});

				var from = date.ToDateTime(TimeOnly.MinValue, DateTimeKind.Local);
				var to = date.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Local);
				_logger.LogInformation("nschedule {nschedule} {from}-{to}", nschedule, from, to);
				var times = nschedule?.GetNextOccurrences(from, to);

				if (times is not null)
				{
					foreach (var time in times)
					{
						foreach (var zone in schedule.StationTimes)
						{
							if (zone.RunLength > TimeSpan.Zero)
							{
								var t = time;
								if (t <= endTime)
								{
									t = endTime.AddSeconds(10);
								}

								var run = new ZoneRun()
								{
									StartDateTime = t,
									ZoneId = zone.StationId,
									RunLength = zone.RunLength
								};
								_logger.LogInformation("Adding Zone Run {ZoneId} {StartDateTime} {Length}", run.ZoneId, run.StartDateTime, run.RunLength);
								await _scheduleRepository.AddZoneRunIfNotFoundAsync(run);
								endTime = t.Add(run.RunLength);
							}
						}
					}
				}
			}
			var n = DateTime.Now;
			var d = DateOnly.FromDateTime(n);
			d = d.AddDays(1);
			var tt = d.ToDateTime(new TimeOnly(1, 0, 0));
			var wait = new TimeSpan(tt.Ticks) - new TimeSpan(n.Ticks);

			_logger.LogInformation("Waiting until {time} to generate again in {length}", tt, wait);
			await Task.Delay(wait, stoppingToken);
		}
	}

	public new void Dispose()
	{
		try
		{
			_scope.Dispose();
		}
		catch
		{

		}
		try
		{
			base.Dispose();
		}
		catch
		{

		}
	}
}
