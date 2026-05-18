using MediatR;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public record GetStatusQuery : IRequest<StatusDto>;
