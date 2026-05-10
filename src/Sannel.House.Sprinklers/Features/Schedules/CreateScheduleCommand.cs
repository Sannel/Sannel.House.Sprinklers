using MediatR;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Features.Schedules;

public record CreateScheduleCommand(NewScheduleDto Schedule) : IRequest<Guid>;
