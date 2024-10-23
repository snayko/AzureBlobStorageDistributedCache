using Microsoft.Extensions.Caching.Distributed;
using Azure.Storage.Blobs;

namespace BlobStorageDistributedCache;

public class AzureBlobStorageDistributedCache
    : IDistributedCache
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly int _defaultTimeInMunitesForAbsoluteExpiration;
    
    public AzureBlobStorageDistributedCache(
        BlobServiceClient blobServiceClient,
        int defaultTimeInMunitesForAbsoluteExpiration = 10)//Read config from Options
    {
        _blobServiceClient = blobServiceClient;
        _defaultTimeInMunitesForAbsoluteExpiration = defaultTimeInMunitesForAbsoluteExpiration;
    }

    public async Task<byte[]?> GetAsync(string key, CancellationToken token = new CancellationToken())
    {
        return await _blobServiceClient.GetBlobAsyncAsByteArray(key,
            invalidateIfExpired: true,
            minutesToCache: _defaultTimeInMunitesForAbsoluteExpiration,
            cancellationToken: token);
    }

    public async Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options,
        CancellationToken token = new CancellationToken())
    {
        var minutesToCache = options.AbsoluteExpirationRelativeToNow?.Minutes ??
                             _defaultTimeInMunitesForAbsoluteExpiration;

        await _blobServiceClient.CreateBlobAsync(key, value, overwrite: true, minutesToCache: minutesToCache,
            cancellationToken: token);
    }
    
    public async Task RemoveAsync(string key, CancellationToken token = new CancellationToken())
    {
        await _blobServiceClient.RemoveBlobAsync(key, cancellationToken: token);
    }
    
    public Task RefreshAsync(string key, CancellationToken token = new CancellationToken())
    {
        //do nothing - sliding expiration is not supported
        return Task.CompletedTask;
    }
    
    public byte[]? Get(string key)
    {
        return GetAsync(key).GetAwaiter().GetResult();
    }
    
    public void Set(string key, byte[] value, DistributedCacheEntryOptions options)
    {
        SetAsync(key, value, options).GetAwaiter().GetResult();
    }
    
    public void Refresh(string key)
    {
        //do nothing - sliding expiration is not supported
    }

    public void Remove(string key)
    {
        RemoveAsync(key).GetAwaiter().GetResult();
    }
}