using Sannel.House.Sprinklers.Features.Messaging;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Features.Zones;

public record GetZonesQuery : IRequest<IEnumerable<ZoneInfoDto>>;
