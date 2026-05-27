using Sannel.House.Sprinklers.Features.Messaging;

namespace Sannel.House.Sprinklers.Features.Sprinklers;

public class EnqueueZonesCommandHandler : IRequestHandler<EnqueueZonesCommand, bool>
{
	private readonly SprinklerWorker _worker;

	public EnqueueZonesCommandHandler(SprinklerWorker worker)
	{
		_worker = worker;
	}

	public async Task<bool> Handle(EnqueueZonesCommand request, CancellationToken cancellationToken)
	{
		await _worker.EnqueueZonesAsync(request.Zones);
		return true;
	}
}
