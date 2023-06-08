using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core.Schedules;
using Sannel.House.Sprinklers.Core.Schedules.Models;
using Sannel.House.Sprinklers.Requests.Schedules;
using Sannel.House.Sprinklers.Responses.Schedules;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class ScheduleController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly IScheduleRepository _scheduleRepository;
	private readonly ILogger _logger;

	public ScheduleController(IScheduleRepository scheduleRepository, ILogger<ScheduleController> logger)
	{
		_scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(List<ScheduleProgramResponse>), (int)HttpStatusCode.OK)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetSchedules()
	{
		var schedules = await _scheduleRepository.GetAllSchedulesAsync();

		return Ok(schedules.Select(i => new ScheduleProgramResponse()
		{
			Id = i.Id,
			Name = i.Name,
			ScheduleCron = i.ScheduleCron,
			StationTimes = i.StationTimes,
			Enabled = i.Enabled,
		}));
	}

	[HttpGet("{id}", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(ScheduleProgramResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetSchedule(Guid id)
	{
		var p = await _scheduleRepository.GetScheduleProgramAsync(id);
		return p is not null
			? Ok(new ScheduleProgramResponse()
			{
				Id = p.Id,
				Name = p.Name,
				ScheduleCron = p.ScheduleCron,
				StationTimes = p.StationTimes,
				Enabled = p.Enabled,
			})
			: NotFound();
	}

	[HttpPost(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> CreateSchedule([FromBody] NewScheduleRequest newScheduleRequest)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var scheduleProgram = new ScheduleProgram
		{
			Id = Guid.NewGuid(),
			Name = newScheduleRequest.Name,
			ScheduleCron = newScheduleRequest.ScheduleCron,
			StationTimes = newScheduleRequest.StationTimes,
		};

		await _scheduleRepository.AddOrUpdateScheduleAsync(scheduleProgram);
		return Ok(scheduleProgram.Id);
	}
	[HttpPut(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> UpdateSchedule([FromBody] UpdateScheduleRequest newScheduleRequest)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var scheduleProgram = new ScheduleProgram
		{
			Id = newScheduleRequest.Id,
			Name = newScheduleRequest.Name,
			ScheduleCron = newScheduleRequest.ScheduleCron,
			StationTimes = newScheduleRequest.StationTimes,
		};

		await _scheduleRepository.AddOrUpdateScheduleAsync(scheduleProgram);
		return Ok(scheduleProgram.Id);
	}

	[HttpPut("{scheduleId}/{isEnable}", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> UpdateScheduleStatus(Guid scheduleId, bool isEnable)
	{
		if (isEnable)
		{
			await _scheduleRepository.EnableScheduleAsync(scheduleId);
		}
		else
		{
			await _scheduleRepository.DisableScheduleAsync(scheduleId);
		}

		return Ok();
	}

	[HttpGet("runs/today", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(IEnumerable<ZoneRunResponse>), (int)HttpStatusCode.OK)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetTodaysRuns()
		=> Ok((await _scheduleRepository.GetRunsByDayAsync(DateOnly.FromDateTime(DateTime.Now))).Select(i => new ZoneRunResponse(i)));

	[HttpGet("runs/{startDate}/{endDate}", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(IEnumerable<ZoneRunResponse>), (int)HttpStatusCode.OK)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetRunRange(DateOnly startDate, DateOnly endDate)
	{
		if (startDate > endDate)
		{
			return BadRequest("StartDate must be before endDate");
		}

		var result = await _scheduleRepository.GetRunsByRangeAsync(startDate, endDate);

		return Ok(result.Select(i => new ZoneRunResponse(i)));
	}

}
