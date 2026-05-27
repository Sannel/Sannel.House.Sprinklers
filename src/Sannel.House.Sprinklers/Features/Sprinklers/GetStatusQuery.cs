using Sannel.House.Sprinklers.Features.Messaging;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public record GetStatusQuery : IRequest<StatusDto>;
