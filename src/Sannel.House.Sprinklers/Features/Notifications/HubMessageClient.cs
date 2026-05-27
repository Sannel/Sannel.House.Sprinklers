using Microsoft.AspNetCore.SignalR;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Features.Notifications;

public class HubMessageClient : IMessageClient
{
	private readonly IHubContext<MessageHub> _hubContext;

	public HubMessageClient(IHubContext<MessageHub> hubContext)
	{
		ArgumentNullException.ThrowIfNull(hubContext);
		_hubContext = hubContext;
	}

	/// <summary>
	/// Fired in-process whenever a progress message is sent, allowing Blazor Server components
	/// to react to live updates without a separate SignalR client connection.
	/// </summary>
	public event Action<StationProgressMessage>? OnProgress;

	/// <summary>
	/// Fired in-process whenever a start message is sent.
	/// </summary>
	public event Action<StationStartMessage>? OnStart;

	/// <summary>
	/// Fired in-process whenever a stop message is sent.
	/// </summary>
	public event Action<StationStopMessage>? OnStop;

	public async Task SendProgressMessageAsync(StationProgressMessage message)
	{
		await _hubContext.Clients.All.SendAsync(EventNames.PROGRESS_MESSAGE, message);
		OnProgress?.Invoke(message);
	}

	public async Task SendStartMessageAsync(StationStartMessage message)
	{
		await _hubContext.Clients.All.SendAsync(EventNames.START_MESSAGE, message);
		OnStart?.Invoke(message);
	}

	public async Task SendStopMessageAsync(StationStopMessage message)
	{
		await _hubContext.Clients.All.SendAsync(EventNames.STOP_MESSAGE, message);
		OnStop?.Invoke(message);
	}

	public async Task SendZoneUpdateMessageAsync(ZoneUpdateMessage message)
		=> await _hubContext.Clients.All.SendAsync(EventNames.ZONE_UPDATE_MESSAGE, message);
}
