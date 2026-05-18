using MediatR;
using Sannel.House.Sprinklers.Shared.Dtos.Logs;

namespace Sannel.House.Sprinklers.Features.Logs;

public record GetRunHistoryQuery(DateOnly From, DateOnly To) : IRequest<IEnumerable<ZoneRunDto>>;
