using MediatR;
using Sannel.House.Sprinklers.Shared.Dtos.Zones;

namespace Sannel.House.Sprinklers.Features.Zones;

public record UpdateZoneCommand(ZoneInfoDto ZoneInfo) : IRequest;
