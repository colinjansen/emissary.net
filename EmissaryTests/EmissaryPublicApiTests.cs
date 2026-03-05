using System.Reflection;
using DevHound.Emissary;
using Microsoft.Extensions.DependencyInjection;

namespace EmissaryTests;

[TestClass]
public sealed class EmissaryPublicApiTests
{
    [TestMethod]
    public void RegisterServicesFromAssemblies_ThrowsOnNull()
    {
        var options = new EmissaryOptions();

        AssertThrows<ArgumentNullException>(() =>
            options.RegisterServicesFromAssemblies(null!));
    }

    [TestMethod]
    public void RegisterServicesFromAssemblies_AddsAssembly()
    {
        var options = new EmissaryOptions();
        var assembly = typeof(EmissaryPublicApiTests).Assembly;

        options.RegisterServicesFromAssemblies(assembly);

        var assembliesProperty = typeof(EmissaryOptions)
            .GetProperty("Assemblies", BindingFlags.Instance | BindingFlags.NonPublic);
        var assemblies = (List<Assembly>)assembliesProperty!.GetValue(options)!;

        Assert.HasCount(1, assemblies);
        Assert.AreSame(assembly, assemblies[0]);
    }

    [TestMethod]
    public void AddEmissary_ThrowsWhenNoAssembliesRegistered()
    {
        var services = new ServiceCollection();

        AssertThrows<InvalidOperationException>(() =>
            services.AddEmissary(_ => { }));
    }

    [TestMethod]
    public void AddEmissary_RegistersAndResolvesHandlers()
    {
        var services = new ServiceCollection();
        services.AddEmissary(cfg =>
            cfg.RegisterServicesFromAssemblies(typeof(PingHandler).Assembly));

        var provider = services.BuildServiceProvider();
        var sender = provider.GetRequiredService<ISender>();

        var result = sender.Send(new PingQuery(41), CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        Assert.AreEqual(42, result);
    }

    [TestMethod]
    public void Emissary_Send_ThrowsOnNullRequest()
    {
        var provider = new ServiceCollection().BuildServiceProvider();
        var emissary = new Emissary(provider);

        AssertThrows<ArgumentNullException>(() =>
            emissary.Send<int>(null!, CancellationToken.None).GetAwaiter().GetResult());
    }

    [TestMethod]
    public void SendCommand_ForwardsToSender()
    {
        var sender = new FakeSender();
        var command = new TestCommand();

        var result = sender.SendCommand(command, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        Assert.AreEqual(42, result);
        Assert.AreSame(command, sender.LastRequest);
    }

    [TestMethod]
    public void SendQuery_ForwardsToSender()
    {
        var sender = new FakeSender();
        var query = new TestQuery();

        var result = sender.SendQuery(query, CancellationToken.None)
            .GetAwaiter()
            .GetResult();

        Assert.AreEqual(42, result);
        Assert.AreSame(query, sender.LastRequest);
    }

    [TestMethod]
    public void SendCommand_ThrowsOnNullSender()
    {
        AssertThrows<ArgumentNullException>(() =>
            SenderExtensions.SendCommand(null!, new TestCommand(), CancellationToken.None)
                .GetAwaiter()
                .GetResult());
    }

    [TestMethod]
    public void SendQuery_ThrowsOnNullSender()
    {
        AssertThrows<ArgumentNullException>(() =>
            SenderExtensions.SendQuery(null!, new TestQuery(), CancellationToken.None)
                .GetAwaiter()
                .GetResult());
    }

    private static void AssertThrows<TException>(Action action)
        where TException : Exception
    {
        try
        {
            action();
            Assert.Fail($"Expected {typeof(TException).Name} was not thrown.");
        }
        catch (TException)
        {
        }
    }

    private sealed record PingQuery(int Value) : IQuery<int>;

    private sealed class PingHandler : IQueryHandler<PingQuery, int>
    {
        public Task<int> Handle(PingQuery request, CancellationToken ct)
        {
            return Task.FromResult(request.Value + 1);
        }
    }

    private sealed record TestCommand : ICommand<int>;

    private sealed record TestQuery : IQuery<int>;

    private sealed class FakeSender : ISender
    {
        public object? LastRequest { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
        {
            LastRequest = request;
            return Task.FromResult((TResponse)(object)42);
        }
    }
}
