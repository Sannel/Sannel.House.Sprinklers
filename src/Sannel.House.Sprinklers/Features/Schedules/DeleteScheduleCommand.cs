using MediatR;

namespace Sannel.House.Sprinklers.Features.Schedules;

public record DeleteScheduleCommand(Guid Id) : IRequest<bool>;
