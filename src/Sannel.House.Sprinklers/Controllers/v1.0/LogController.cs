using Asp.Versioning;
using Sannel.House.Sprinklers.Features.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Features.Logs;
using Sannel.House.Sprinklers.Shared.Dtos.Logs;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
[Authorize(AuthPolicy.ZONE_READERS)]
public class LogController : ControllerBase
{
private const string VERSION = "v1";
private readonly IMediator _mediator;

public LogController(IMediator mediator)
{
_mediator = mediator;
}

[HttpGet("Runs/{from}/{to}", Name = $"{VERSION}.[controller].[action]")]
[ProducesResponseType(typeof(IEnumerable<ZoneRunDto>), 200)]
public async Task<IActionResult> GetRunsForRange(DateOnly from, DateOnly to)
{
if (from > to)
return BadRequest("StartDate must be before endDate");

var logs = await _mediator.Send(new GetRunHistoryQuery(from, to));
return Ok(logs);
}
}
