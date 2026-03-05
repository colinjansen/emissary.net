using System.Threading;
using System.Threading.Tasks;

namespace DevHound.Emissary;

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default);
}
