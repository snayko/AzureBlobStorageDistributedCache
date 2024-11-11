namespace WebAppTestAzureBlobDistributedCache.Extensions;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddAzureRedisDistributedCache(this IServiceCollection services, string redisConnectionString)
    {
        // Register Redis cache in the DI container
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisConnectionString; // Connection string to Azure Redis Cache
            //options.InstanceName = "SampleAppCache"; // Optional, can be used for multi-instance Redis setups
        });
        
        return services;
    }
}