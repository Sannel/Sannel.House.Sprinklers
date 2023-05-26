using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Sannel.House.Sprinklers.Core.Schedules;
using Sannel.House.Sprinklers.Core.Schedules.Models;
using Sannel.House.Sprinklers.Requests.Schedules;
using Sannel.House.Sprinklers.Responses.Schedules;

namespace Sannel.House.Sprinklers.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ScheduleController : ControllerBase
{
	private readonly IScheduleRepository _scheduleRepository;
	private readonly ILogger _logger;

	public ScheduleController(IScheduleRepository scheduleRepository, ILogger<ScheduleController> logger)
	{
		_scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpGet]
	[ProducesResponseType(typeof(List<ScheduleProgramResponse>), (int)HttpStatusCode.OK)]
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

	[HttpGet("{id}")]
	[ProducesResponseType(typeof(ScheduleProgramResponse), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<IActionResult> GetSchedule(Guid id)
	{
		var p = await _scheduleRepository.GetScheduleProgramAsync(id);
		if(p is not null)
		{
			return Ok(new ScheduleProgramResponse()
			{
				Id = p.Id,
				Name = p.Name,
				ScheduleCron = p.ScheduleCron,
				StationTimes = p.StationTimes,
				Enabled = p.Enabled,
			});
		}
		else
		{
			return NotFound();
		}
	}

	[HttpPost]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
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
	[HttpPut]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
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

	[HttpPut("{scheduleId}/{isEnable}")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	public async Task<IActionResult> UpdateScheduleStatus(Guid scheduleId, bool isEnable)
	{
		if(isEnable)
		{
			await _scheduleRepository.EnableScheduleAsync(scheduleId);
		}
		else
		{
			await _scheduleRepository.DisableScheduleAsync(scheduleId);
		}

		return Ok();
	}

	[HttpGet("runs/today")]
	[ProducesResponseType(typeof(IEnumerable<ZoneRunResponse>), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> GetTodaysRuns()
		=> Ok((await _scheduleRepository.GetRunsByDayAsync(DateOnly.FromDateTime(DateTime.Now))).Select(i => new ZoneRunResponse(i)));

	[HttpGet("runs/{startDate}/{endDate}")]
	[ProducesResponseType(typeof(IEnumerable<ZoneRunResponse>), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> GetRunRange(DateOnly startDate, DateOnly endDate)
	{
		if(startDate > endDate)
		{
			return BadRequest("StartDate must be before endDate");
		}

		var result = await _scheduleRepository.GetRunsByRangeAsync(startDate, endDate);

		return Ok(result.Select(i => new ZoneRunResponse(i)));
	}
	
}
