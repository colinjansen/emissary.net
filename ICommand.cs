namespace DevHound.Emissary;

public interface ICommand<out TResponse> : IRequest<TResponse>
{
}
