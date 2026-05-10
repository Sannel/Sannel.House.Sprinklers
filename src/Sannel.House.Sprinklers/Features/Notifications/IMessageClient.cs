using Sannel.House.Sprinklers.Shared.Messages;

namespace Sannel.House.Sprinklers.Features.Notifications;

public interface IMessageClient
{
	Task SendStartMessageAsync(StationStartMessage message);
	Task SendStopMessageAsync(StationStopMessage message);
	Task SendProgressMessageAsync(StationProgressMessage message);
	Task SendZoneUpdateMessageAsync(ZoneUpdateMessage message);
}
