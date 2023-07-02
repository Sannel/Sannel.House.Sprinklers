using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Infrastructure;
public class MQTTMessageClient : IMessageClient
{
	private readonly MQTTManager _manager;
	private readonly ILogger _logger;

	public MQTTMessageClient(MQTTManager manager, ILogger<MQTTMessageClient> logger)
	{
		ArgumentNullException.ThrowIfNull(manager);
		ArgumentNullException.ThrowIfNull(logger);
		_manager = manager;
		_logger = logger;
	}

	public Task SendProgressMessageAsync(StationProgressMessage message)
	{
		const string topic = $"Sannel/House/Sprinklers/{EventNames.PROGRESS_MESSAGE}";
		try
		{
			var mqtt = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(JsonSerializer.Serialize(message))
				.Build();

			return _manager.PublishAsync(mqtt);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sending progress message {zoneId}", message.ZoneId);
			return Task.CompletedTask;
		}
	}

	public async Task SendStartMessageAsync(StationStartMessage message)
	{
		const string topic = $"Sannel/House/Sprinklers/{EventNames.START_MESSAGE}";
		try
		{
			var mqtt = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(JsonSerializer.Serialize(message))
				.Build();

			await _manager.PublishAsync(mqtt);
		}
		catch (Exception ex)
		{
			_logger.LogError(ex, "Error sending start message {zoneId}", message.ZoneId);
		}
	}

	public async Task SendStopMessageAsync(StationStopMessage message)
	{
		const string topic = $"Sannel/House/Sprinklers/{EventNames.STOP_MESSAGE}";
		try
		{
			var mqtt = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(JsonSerializer.Serialize(message))
				.Build();

			await _manager.PublishAsync(mqtt);
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Error sending stop message {zoneId}", message.ZoneId);
		}
	}

	public async Task SendZoneUpdateMessageAsync(ZoneUpdateMessage message)
	{
		const string topic = $"Sannel/House/Sprinklers/{EventNames.ZONE_UPDATE_MESSAGE}";
		try
		{
			var mqtt = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(JsonSerializer.Serialize(message))
				.Build();

			await _manager.PublishAsync(mqtt);
		}
		catch(Exception ex)
		{
			_logger.LogError(ex, "Error sending Zone Update Message {zoneId}", message?.ZoneInfo?.ZoneId);
		}
	}
}
