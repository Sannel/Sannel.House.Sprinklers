using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Core.Zones;
using Sannel.House.Sprinklers.Mappers;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Controllers.v1_0;

[Route("sprinkler/api/v{version:apiVersion}/[controller]")]
[ApiController]
[ApiVersion(1.0)]
public class ZoneController : ControllerBase
{
	private const string VERSION = "v1";
	private readonly IZoneRepository _zoneRepository;
	private readonly ZoneInfoMapper _zoneInfoMapper;
	private readonly ILoggerRepository _loggerRepository;

	public ZoneController(IZoneRepository zoneRepository,
		ZoneInfoMapper zoneInfoMapper,
		ILoggerRepository loggerRepository)
	{
		ArgumentNullException.ThrowIfNull(zoneRepository);
		ArgumentNullException.ThrowIfNull(zoneInfoMapper);
		ArgumentNullException.ThrowIfNull(loggerRepository);
		_zoneRepository = zoneRepository;
		_zoneInfoMapper = zoneInfoMapper;
		_loggerRepository = loggerRepository;
	}

	[HttpGet(Name = $"{VERSION}.[controller].[action]")]
	[Authorize(AuthPolicy.ZONE_METADATA_READER)]
	[ProducesResponseType(typeof(IEnumerable<ZoneInfoDto>), (int)HttpStatusCode.OK)]
	public async Task<IActionResult> GetAllZoneMetaData()
	{
		var zones = await _zoneRepository.GetAllZones();

		return Ok(zones.Select(i => _zoneInfoMapper.ModelToDto(i)));
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
			return Ok(_zoneInfoMapper.ModelToDto(metaData));
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
		await _zoneRepository.AddOrUpdateZoneInfoAsync(_zoneInfoMapper.DtoToModel(zoneInfo));

		return Ok();
	}

}
