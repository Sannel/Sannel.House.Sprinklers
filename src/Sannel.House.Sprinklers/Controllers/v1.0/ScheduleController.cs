using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core.Schedules;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class ScheduleController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly IScheduleRepository _scheduleRepository;
	private readonly ScheduleMapper _mapper;
	private readonly ILogger _logger;

	public ScheduleController(IScheduleRepository scheduleRepository, ScheduleMapper mapper, ILogger<ScheduleController> logger)
	{
		_scheduleRepository = scheduleRepository ?? throw new ArgumentNullException(nameof(scheduleRepository));
		_mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
		_logger = logger ?? throw new ArgumentNullException(nameof(logger));
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(List<ScheduleProgramDto>), (int)HttpStatusCode.OK)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetSchedules()
	{
		var schedules = await _scheduleRepository.GetAllSchedulesAsync();

		return Ok(schedules.Select(i => _mapper.ModelToDto(i)));
	}

	[HttpGet("{id}", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(ScheduleProgramDto), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	[Authorize(AuthPolicy.SCHEDULE_READERS)]
	public async Task<IActionResult> GetSchedule(Guid id)
	{
		var p = await _scheduleRepository.GetScheduleProgramAsync(id);
		return p is not null
			? Ok(_mapper.ModelToDto(p))
			: NotFound();
	}

	[HttpPost(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> CreateSchedule([FromBody] NewScheduleDto newScheduleRequest)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var scheduleProgram = _mapper.DtoToModel(newScheduleRequest);

		await _scheduleRepository.AddOrUpdateScheduleAsync(scheduleProgram);
		return Ok(scheduleProgram.Id);
	}
	[HttpPut(Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(Guid), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.BadRequest)]
	[Authorize(AuthPolicy.SCHEDULE_SCHEDULERS)]
	public async Task<IActionResult> UpdateSchedule([FromBody] UpdateScheduleDto newScheduleRequest)
	{
		if (!ModelState.IsValid)
		{
			return BadRequest(ModelState);
		}

		var scheduleProgram = _mapper.DtoToModel(newScheduleRequest);

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
}
