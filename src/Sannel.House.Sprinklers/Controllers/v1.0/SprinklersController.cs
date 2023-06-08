using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core.Hardware;
using Sannel.House.Sprinklers.Responses.Sprinklers;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class SprinklersController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly ILogger _logger;
	private readonly SprinklerService _service;

	public SprinklersController(SprinklerService service, ILogger<SprinklersController> logger)
	{
		_service = service;
		_logger = logger;
	}

	[HttpPost("Start", Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_TRIGGERS)]
	[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> Start(byte zoneId, TimeSpan length)
	{
		var result = await _service.StartZoneAsync(zoneId, length);

		return Ok(result);
	}

	[HttpPost("Stop", Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_TRIGGERS)]
	[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> Stop()
	{
		var result = await _service.StopAllAsync();
		return Ok(result);
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_READERS)]
	[ProducesResponseType(typeof(Status), (int)HttpStatusCode.OK)]
	public IActionResult Status()
	{
		var status = new Status()
		{
			IsRunning = _service.IsRunning,
			TimeLeft = _service.TimeLeft,
			Zones = _service.Zones
		};

		if (status.IsRunning)
		{
			status.RunningZone = _service.StationId;
		}

		return Ok(status);
	}
}
