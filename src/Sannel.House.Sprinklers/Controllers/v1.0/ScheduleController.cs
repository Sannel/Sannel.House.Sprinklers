using System.Net;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Features.Schedules;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class ScheduleController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly IMediator _mediator;

	public ScheduleController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(List<ScheduleProgramDto>), (int)HttpStatusCode.OK)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetSchedules()
	{
		var schedules = await _mediator.Send(new GetSchedulesQuery());
		return Ok(schedules);
	}

	[HttpGet("{id}", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(ScheduleProgramDto), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetSchedule(Guid id)
	{
		var schedule = await _mediator.Send(new GetScheduleByIdQuery(id));
		return schedule is not null ? Ok(schedule) : NotFound();
	}

	[HttpPost(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> CreateSchedule([FromBody] NewScheduleDto newScheduleRequest)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var id = await _mediator.Send(new CreateScheduleCommand(newScheduleRequest));
		return Ok(id);
	}

	[HttpPut(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> UpdateSchedule([FromBody] UpdateScheduleDto updateScheduleRequest)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);

		var id = await _mediator.Send(new UpdateScheduleCommand(updateScheduleRequest));
		return Ok(id);
	}

	[HttpPut("{scheduleId}/{isEnable}", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> UpdateScheduleStatus(Guid scheduleId, bool isEnable)
	{
		await _mediator.Send(new EnableScheduleCommand(scheduleId, isEnable));
		return Ok();
	}
}

