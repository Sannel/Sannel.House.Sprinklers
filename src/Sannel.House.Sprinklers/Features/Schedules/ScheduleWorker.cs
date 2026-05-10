using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sannel.House.Sprinklers.Features.Common;
using Sannel.House.Sprinklers.Features.Sprinklers;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;

namespace Sannel.House.Sprinklers.Features.Schedules;

public class ScheduleWorker : BackgroundService
{
	private readonly IServiceProvider _provider;
	private readonly ILogger<ScheduleWorker> _logger;

	public ScheduleWorker(IServiceProvider provider, ILogger<ScheduleWorker> logger)
	{
		_provider = provider;
		_logger = logger;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		using (var scope = _provider.CreateScope())
		{
			await GenerateScheduleAsync(scope.ServiceProvider, stoppingToken);
		}

		_ = Task.Run(() => RunScheduleGenerationLoopAsync(stoppingToken), stoppingToken);

		while (!stoppingToken.IsCancellationRequested)
		{
			try
			{
				using var scope = _provider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<SprinklerDbContext>();
				var worker = scope.ServiceProvider.GetRequiredService<SprinklerWorker>();

				if (!worker.IsRunning)
				{
					var today = DateOnly.FromDateTime(DateTime.Now);
					var startOfDay = today.ToDateTime(TimeOnly.MinValue);
					var endOfDay = today.ToDateTime(TimeOnly.MaxValue);

					var first = await db.Runs
						.Where(r => r.StartDateTime >= startOfDay
								 && r.StartDateTime <= endOfDay
								 && !r.Ran
								 && r.StartDateTime <= DateTime.Now)
						.OrderBy(r => r.StartDateTime)
						.FirstOrDefaultAsync(stoppingToken);

					if (first is not null)
					{
						first.Ran = true;
						first.StartedAt = DateTime.Now;
						await db.SaveChangesAsync(stoppingToken);

						await worker.EnqueueZonesAsync(
							new[] { new ZoneStartRequestDto { ZoneId = first.ZoneId, Length = first.RunLength } });
					}
				}
			}
			catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
			{
				break;
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Error in ScheduleWorker poll loop");
			}

			await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
		}
	}

	private async Task RunScheduleGenerationLoopAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			var now = DateTime.Now;
			var next1AM = DateOnly.FromDateTime(now).AddDays(1).ToDateTime(new TimeOnly(1, 0, 0));
			var delay = next1AM - now;

			_logger.LogInformation("Next schedule generation at {Time} (in {Delay})", next1AM, delay);
			await Task.Delay(delay, stoppingToken);

			if (!stoppingToken.IsCancellationRequested)
			{
				using var scope = _provider.CreateScope();
				await GenerateScheduleAsync(scope.ServiceProvider, stoppingToken);
			}
		}
	}

	private async Task GenerateScheduleAsync(IServiceProvider services, CancellationToken stoppingToken)
	{
		_logger.LogInformation("Generating schedule for today");
		var db = services.GetRequiredService<SprinklerDbContext>();
		var today = DateOnly.FromDateTime(DateTime.Today);

		var schedules = await db.Programs
			.Where(p => p.Enabled)
			.ToListAsync(stoppingToken);

		foreach (var schedule in schedules)
		{
			if (!IsDueToday(schedule, today))
				continue;

			_logger.LogInformation("Generating runs for schedule {Name}", schedule.Name);
			var runTime = today.ToDateTime(schedule.StartTime);

			foreach (var station in schedule.StationTimes.OrderBy(s => s.StationId))
			{
				if (station.RunLength <= TimeSpan.Zero)
					continue;

				var startDt = runTime;
				if (!await db.Runs.AnyAsync(
						r => r.StartDateTime == startDt && r.ZoneId == station.StationId,
						stoppingToken))
				{
					await db.Runs.AddAsync(new ZoneRun
					{
						StartDateTime = startDt,
						ZoneId = station.StationId,
						RunLength = station.RunLength
					}, stoppingToken);

					_logger.LogInformation("Added ZoneRun zone={ZoneId} at {Time}", station.StationId, startDt);
				}

				runTime = runTime.Add(station.RunLength).AddSeconds(10);
			}
		}

		await db.SaveChangesAsync(stoppingToken);
	}

	private static bool IsDueToday(ScheduleProgram schedule, DateOnly today)
	{
		if (schedule.DaysOfWeek is { Count: > 0 })
			return schedule.DaysOfWeek.Contains(today.DayOfWeek);

		if (schedule.IntervalDays is not null && schedule.IntervalStartDate is not null)
			return (today.DayNumber - schedule.IntervalStartDate.Value.DayNumber) % schedule.IntervalDays.Value == 0;

		return false;
	}
}
