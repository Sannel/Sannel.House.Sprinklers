using System.Device.Gpio;
using System.Device.Gpio.Drivers;
using System.Device.Spi;
using Iot.Device.Board;
using Iot.Device.Multiplexing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Sannel.House.Sprinklers.Core.Hardware;
using Sannel.House.Sprinklers.Responses.Sprinklers;

namespace Sannel.House.Sprinklers.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class SprinklersController : ControllerBase
	{

		private readonly ILogger logger;
		private readonly SprinklerService _service;

		public SprinklersController(SprinklerService service, ILogger<SprinklersController> logger)
		{
			_service = service;
			this.logger = logger;
		}

		[HttpPost("Start")]
		public async Task<IActionResult> Start(byte zoneId, string length)
		{
			if(!TimeSpan.TryParse(length, out var time)) 
			{
				return BadRequest("length is not a valid TimeSpan");
			}
			var result = await _service.StartZoneAsync(zoneId, time);

			return Ok(result);
		}

		[HttpPost("Stop")]
		public async Task<IActionResult> Stop()
		{
			var result = await _service.StopAllAsync();
			return Ok(result);
		}

		[HttpGet]
		public IActionResult Status()
		{
			var status = new Status()
			{
				IsRunning = _service.IsRunning,
				TimeLeft = _service.TimeLeft,
				Zones = _service.Zones
			};

			if(status.IsRunning)
			{
				status.RunningZone = _service.StationId;
			}

			return Ok(status);
		}
	}
}
