using System.Net;
using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Features.Sprinklers;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class SprinklersController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly IMediator _mediator;

	public SprinklersController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpPost("Start", Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_TRIGGERS)]
	[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> Start([FromBody] IEnumerable<ZoneStartRequestDto> zones)
	{
		var result = await _mediator.Send(new EnqueueZonesCommand(zones));
		return Ok(result);
	}

	[HttpPost("Stop", Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_TRIGGERS)]
	[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> Stop()
	{
		var result = await _mediator.Send(new StopAllCommand());
		return Ok(result);
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_READERS)]
	[ProducesResponseType(typeof(StatusDto), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> Status()
	{
		var status = await _mediator.Send(new GetStatusQuery());
		return Ok(status);
	}
}

