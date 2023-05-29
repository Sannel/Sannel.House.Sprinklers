using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core.Hardware;
using Sannel.House.Sprinklers.Responses.Sprinklers;

namespace Sannel.House.Sprinklers.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SprinklersController : ControllerBase
{

	private readonly ILogger _logger;
	private readonly SprinklerService _service;

	public SprinklersController(SprinklerService service, ILogger<SprinklersController> logger)
	{
		_service = service;
		_logger = logger;
	}

	[HttpPost("Start")]
	[Authorize(AuthPolicy.ZONE_TRIGGERS)]
	[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> Start(byte zoneId, TimeSpan length)
	{
		var result = await _service.StartZoneAsync(zoneId, length);

		return Ok(result);
	}

	[HttpPost("Stop")]
	[Authorize(AuthPolicy.ZONE_TRIGGERS)]
	[ProducesResponseType(typeof(bool), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> Stop()
	{
		var result = await _service.StopAllAsync();
		return Ok(result);
	}

	[HttpGet]
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
