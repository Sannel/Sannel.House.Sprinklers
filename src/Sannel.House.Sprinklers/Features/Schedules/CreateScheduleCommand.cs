using Sannel.House.Sprinklers.Features.Messaging;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Features.Schedules;

public record CreateScheduleCommand(NewScheduleDto Schedule) : IRequest<Guid>;
