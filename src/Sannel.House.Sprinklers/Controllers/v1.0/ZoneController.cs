using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Core.Zones;
using Sannel.House.Sprinklers.Responses.Zones;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class ZoneController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly IZoneRepository _zoneRepository;
	private readonly ILoggerRepository _loggerRepository;

	public ZoneController(IZoneRepository zoneRepository, ILoggerRepository loggerRepository)
	{
		ArgumentNullException.ThrowIfNull(zoneRepository);
		ArgumentNullException.ThrowIfNull(loggerRepository);
		_zoneRepository = zoneRepository;
		_loggerRepository = loggerRepository;
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_METADATA_READER)]
	[ProducesResponseType(typeof(IEnumerable<ZoneInfoDto>), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> GetAllZoneMetaData()
	{
		var zones = await _zoneRepository.GetAllZones();

		return Ok(zones.Select(i => new ZoneInfoDto()
		{
			ZoneId = i.ZoneId,
			Name = i.Name,
			Color = i.Color,
		}));
	}

	[HttpGet("{id}", Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_METADATA_READER)]
	[ProducesResponseType(typeof(ZoneInfoDto), (int)HttpStatusCode.OK)]
	[ProducesResponseType((int)HttpStatusCode.NotFound)]
	public async Task<IActionResult> GetZoneMetaData(byte id)
	{
		var metaData = await _zoneRepository.GetZoneInfoByIdAsync(id);

		if (metaData is not null)
		{
			return Ok(new ZoneInfoDto()
			{
				ZoneId = metaData.ZoneId,
				Name = metaData.Name,
				Color = metaData.Color,
			});
		}
		else
		{
			return NotFound();
		}
	}

	[HttpPut(Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_METADATA_WRITER)]
	[ProducesResponseType((int)HttpStatusCode.OK)]
	public async Task<IActionResult> UpdateZoneMetaData([FromBody] ZoneInfoDto zoneInfo)
	{
		var info = new ZoneMetaData()
		{
			ZoneId = zoneInfo.ZoneId,
			Name = zoneInfo.Name,
			Color = zoneInfo.Color,
		};

		await _zoneRepository.AddOrUpdateZoneInfoAsync(info);

		return Ok();
	}

}
