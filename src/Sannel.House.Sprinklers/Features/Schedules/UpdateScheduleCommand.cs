using MediatR;
using Sannel.House.Sprinklers.Shared.Dtos.Schedules;

namespace Sannel.House.Sprinklers.Features.Schedules;

public record UpdateScheduleCommand(UpdateScheduleDto Schedule) : IRequest<Guid>;
