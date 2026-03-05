using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DevHound.Emissary;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEmissary(this IServiceCollection services, Action<EmissaryOptions> configure)
    {
        if (services is null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        var options = new EmissaryOptions();
        configure?.Invoke(options);

        if (options.Assemblies.Count == 0)
        {
            throw new InvalidOperationException("No assemblies registered for emissary.");
        }

        services.AddTransient<ISender, Emissary>();

        foreach (var type in GetCandidateTypes(options.Assemblies))
        {
            foreach (var handlerInterface in type.ImplementedInterfaces
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>)))
            {
                services.AddTransient(handlerInterface, type.AsType());
            }
        }

        return services;
    }

    private static IEnumerable<TypeInfo> GetCandidateTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .Where(a => a is not null)
            .Distinct()
            .SelectMany(a => a.DefinedTypes)
            .Where(t => !t.IsAbstract && !t.IsInterface);
    }
}
