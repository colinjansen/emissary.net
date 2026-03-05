namespace DevHound.Emissary;

public interface IQuery<out TResponse> : IRequest<TResponse>
{
}
