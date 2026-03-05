using System;
using System.Collections.Generic;
using System.Reflection;

namespace DevHound.Emissary;

public sealed class EmissaryOptions
{
    internal List<Assembly> Assemblies { get; } = new();

    public void RegisterServicesFromAssemblies(params Assembly[] assemblies)
    {
        if (assemblies is null)
        {
            throw new ArgumentNullException(nameof(assemblies));
        }

        foreach (var assembly in assemblies)
        {
            if (assembly is not null)
            {
                Assemblies.Add(assembly);
            }
        }
    }
}
