using Martina.Abstractions;
using Martina.Services;

namespace Martina.Extensions;

public static class ServiceCollectionsExtensions
{
    public static void AddBuptSchedular(this IServiceCollection collection)
    {
        collection.AddSingleton<BuptSchedular>();
        collection.AddSingleton<ISchedular, BuptSchedular>(provider => provider.GetRequiredService<BuptSchedular>());
        collection.AddHostedService<BuptSchedular>(provider => provider.GetRequiredService<BuptSchedular>());
    }
}
