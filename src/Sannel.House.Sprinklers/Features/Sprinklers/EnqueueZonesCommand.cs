using Sannel.House.Sprinklers.Features.Messaging;
using Sannel.House.Sprinklers.Shared.Dtos.Sprinklers;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public record EnqueueZonesCommand(IEnumerable<ZoneStartRequestDto> Zones) : IRequest<bool>;
