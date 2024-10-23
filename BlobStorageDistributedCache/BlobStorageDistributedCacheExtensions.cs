using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BlobStorageDistributedCache;

public static class BlobStorageDistributedCacheExtensions
{
    public static IServiceCollection AddAzureBlobDistributedCache(this IServiceCollection services)
    {
        if (services == null)
        {
            throw new ArgumentNullException(nameof(services));
        }

        services.AddOptions();
        
        services.TryAdd(ServiceDescriptor.Singleton<IDistributedCache, AzureBlobStorageDistributedCache>());

        return services;
    }
}