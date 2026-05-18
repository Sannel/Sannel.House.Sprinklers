using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sannel.House.Sprinklers.Features.Notifications;

[Authorize]
public class MessageHub : Hub
{
}
