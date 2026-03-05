using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace DevHound.Emissary;

public sealed class Emissary(IServiceProvider provider) : ISender
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        if (request is null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = provider.GetRequiredService(handlerType);
        var method = handlerType.GetMethod("Handle");

        if (method is null)
        {
            throw new InvalidOperationException($"Handler for {request.GetType().Name} is missing Handle.");
        }

        var result = method.Invoke(handler, new object[] { request, ct });
        if (result is Task<TResponse> task)
        {
            return task;
        }

        throw new InvalidOperationException($"Handler for {request.GetType().Name} returned invalid task.");
    }
}
