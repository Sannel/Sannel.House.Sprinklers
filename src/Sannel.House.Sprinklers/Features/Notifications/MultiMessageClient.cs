using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Features.Notifications;

public class MultiMessageClient : IMessageClient
{
	private readonly IEnumerable<IMessageClient> _clients;

	public MultiMessageClient(params IMessageClient[] clients)
	{
		ArgumentNullException.ThrowIfNull(clients);
		_clients = clients;
	}

	public Task SendProgressMessageAsync(StationProgressMessage message)
		=> Task.WhenAll(_clients.Select(i => i.SendProgressMessageAsync(message)));

	public async Task SendStartMessageAsync(StationStartMessage message)
		=> await Task.WhenAll(_clients.Select(i => i.SendStartMessageAsync(message)));

	public Task SendStopMessageAsync(StationStopMessage message)
		=> Task.WhenAll(_clients.Select(i => i.SendStopMessageAsync(message)));

	public Task SendZoneUpdateMessageAsync(ZoneUpdateMessage message)
		=> Task.WhenAll(_clients.Select(i => i.SendZoneUpdateMessageAsync(message)));
}
