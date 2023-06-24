using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Sannel.House.Sprinklers.Core;
using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Infrastructure;
public class HubMessageClient : Hub, IMessageClient
{
	public async Task SendStartMessageAsync(StationStartMessage message)
	{
		if (Clients?.All is not null)
		{
			await Clients.All.SendAsync("StartMessage", message);
		}
	}

	public async Task SendStopMessageAsync(StationStopMessage message)
	{
		if (Clients?.All is not null)
		{
			await Clients.All.SendAsync("StopMessage", message);
		}
	}
}
