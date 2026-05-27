using Sannel.House.Sprinklers.Features.Messaging;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public record StopAllCommand : IRequest<bool>;
