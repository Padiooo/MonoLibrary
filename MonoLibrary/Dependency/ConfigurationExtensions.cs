using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using System;
using System.Linq;
using System.Reflection;

namespace MonoLibrary.Dependency;

public static class ConfigurationExtensions
{
    public static IServiceCollection AddSettings(this IServiceCollection services, Assembly assembly, IConfiguration configuration)
    {
        var settings = assembly.GetExportedTypes()
            .Select(t => new ValueTuple<SettingsAttribute, Type>(t.GetCustomAttribute<SettingsAttribute>(), t))
            .Where(vt => vt.Item1 is not null)
            ;

        var call = typeof(OptionsConfigurationServiceCollectionExtensions)
            .GetMethod(nameof(OptionsConfigurationServiceCollectionExtensions.Configure), [typeof(IServiceCollection), typeof(IConfiguration)]);

        foreach (var vt in settings)
        {
            var genericCall = call.MakeGenericMethod(vt.Item2);
            var sectionName = string.IsNullOrEmpty(vt.Item1.Name) ? vt.Item2.Name : vt.Item1.Name;
            var section = configuration.GetSection(sectionName);

            genericCall.Invoke(null, [services, section]);
        }

        return services;
    }
}
