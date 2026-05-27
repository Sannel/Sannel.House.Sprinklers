using Sannel.House.Sprinklers.Features.Messaging;

namespace Sannel.House.Sprinklers.Features.Schedules;

public record DeleteScheduleCommand(Guid Id) : IRequest<bool>;
