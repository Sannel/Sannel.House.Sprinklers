using MediatR;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public record StopAllCommand : IRequest<bool>;
