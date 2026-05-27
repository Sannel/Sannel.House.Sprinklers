using System.Net;
using Asp.Versioning;
using Sannel.House.Sprinklers.Features.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Features.Zones;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class ZoneController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly IMediator _mediator;

	public ZoneController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_METADATA_READER)]
	[ProducesResponseType(typeof(IEnumerable<ZoneInfoDto>), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> GetAllZoneMetaData()
	{
		var zones = await _mediator.Send(new GetZonesQuery());
		return Ok(zones);
	}

	[HttpGet("{id}", Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_METADATA_READER)]
	[ProducesResponseType(typeof(ZoneInfoDto), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<IActionResult> GetZoneMetaData(byte id)
	{
		var zone = await _mediator.Send(new GetZoneByIdQuery(id));
		return zone is not null ? Ok(zone) : NotFound();
	}

	[HttpPut(Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_METADATA_WRITER)]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	public async Task<IActionResult> UpdateZoneMetaData([FromBody] ZoneInfoDto zoneInfo)
	{
		await _mediator.Send(new UpdateZoneCommand(zoneInfo));
		return Ok();
	}
}

