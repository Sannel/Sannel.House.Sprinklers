using Sannel.House.Sprinklers.Features.Messaging;

namespace Sannel.House.Sprinklers.Features.Schedules;

public record EnableScheduleCommand(Guid Id, bool IsEnabled) : IRequest;
