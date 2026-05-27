using Sannel.House.Sprinklers.Features.Messaging;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Features.Schedules;

public record GetScheduleByIdQuery(Guid Id) : IRequest<ScheduleProgramDto?>;
