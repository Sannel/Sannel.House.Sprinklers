using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Infrastructure;
public class HubMessageClient : IMessageClient
{
	private readonly IHubContext<MessageHub> _hubContext;

	public HubMessageClient(IHubContext<MessageHub> hubContext)
	{
		ArgumentNullException.ThrowIfNull(hubContext);
		_hubContext = hubContext;
	}

	public async Task SendProgressMessageAsync(StationProgressMessage message)
		=> await _hubContext.Clients.All.SendAsync("ProgressMessage", message);

	public async Task SendStartMessageAsync(StationStartMessage message)
	{
		await _hubContext.Clients.All.SendAsync("StartMessage", message);
	}

	public async Task SendStopMessageAsync(StationStopMessage message)
	{
		await _hubContext.Clients.All.SendAsync("StopMessage", message);
	}
}
