using Sannel.House.Sprinklers.Features.Messaging;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public class StopAllCommandHandler : IRequestHandler<StopAllCommand, bool>
{
	private readonly SprinklerWorker _worker;

	public StopAllCommandHandler(SprinklerWorker worker)
	{
		_worker = worker;
	}

	public async Task<bool> Handle(StopAllCommand request, CancellationToken cancellationToken)
	{
		await _worker.StopAllAsync();
		return true;
	}
}
