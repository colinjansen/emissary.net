using System;
using System.Threading;
using System.Threading.Tasks;

namespace DevHound.Emissary;

public static class SenderExtensions
{
    public static Task<TResponse> SendCommand<TResponse>(this ISender sender, ICommand<TResponse> command, CancellationToken ct = default)
    {
        if (sender is null)
        {
            throw new ArgumentNullException(nameof(sender));
        }

        return sender.Send(command, ct);
    }

    public static Task<TResponse> SendQuery<TResponse>(this ISender sender, IQuery<TResponse> query, CancellationToken ct = default)
    {
        if (sender is null)
        {
            throw new ArgumentNullException(nameof(sender));
        }

        return sender.Send(query, ct);
    }
}
