using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Responses.Logs;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
[Authorize(AuthPolicy.ZONE_READERS)]
public class LogController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly ILoggerRepository _loggerRepository;

	public LogController(ILoggerRepository loggerRepository)
	{
		ArgumentNullException.ThrowIfNull(loggerRepository);
		_loggerRepository = loggerRepository;
	}

	[HttpGet("Runs/{from}/{to}", Name = $"{VERSION}.[controller].[action]")]
	[ProducesResponseType(typeof(IEnumerable<ZoneRunDto>), 200)]
	public async Task<IActionResult> GetRunsForRange(DateOnly from, DateOnly to)
	{
		if (from > to)
		{
			return BadRequest("StartDate must be before endDate");
		}

		var start = from.ToDateTime(TimeOnly.MinValue);
		var end = to.ToDateTime(TimeOnly.MaxValue);

		var now = DateTimeOffset.Now;

		var logs = await _loggerRepository.GetLogs(new DateTimeOffset(start, now.Offset),
									new DateTimeOffset(end, now.Offset),
									LogActions.START);

		return Ok(logs.Select(i => new ZoneRunDto()
		{
			ActionDate = i.ActionDate,
			StationId = i.ZoneId,
			RunLength = i.RunLength
		}));
	}
}
