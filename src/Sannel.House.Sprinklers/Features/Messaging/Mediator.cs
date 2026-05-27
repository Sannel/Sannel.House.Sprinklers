namespace Sannel.House.Sprinklers.Features.Messaging;

public class Mediator : IMediator
{
	private readonly IServiceProvider _serviceProvider;

	public Mediator(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
	{
		var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
		var handler = _serviceProvider.GetRequiredService(handlerType);
		var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest<TResponse>, TResponse>.Handle))!;
		return (Task<TResponse>)method.Invoke(handler, [request, cancellationToken])!;
	}

	public Task Send(IRequest request, CancellationToken cancellationToken = default)
	{
		var handlerType = typeof(IRequestHandler<>).MakeGenericType(request.GetType());
		var handler = _serviceProvider.GetRequiredService(handlerType);
		var method = handlerType.GetMethod(nameof(IRequestHandler<IRequest>.Handle))!;
		return (Task)method.Invoke(handler, [request, cancellationToken])!;
	}
}
