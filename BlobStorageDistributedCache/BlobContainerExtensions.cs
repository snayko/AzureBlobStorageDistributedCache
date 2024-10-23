using System.Text.Json;
using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BlobStorageDistributedCache;

public static class BlobContainerExtensions
{
    // The container name must be lowercase
    public const string DefaultCacheContainerName = "distributed-cache-container";

    //-------------------------------------------------
    // Create a container
    //-------------------------------------------------
    public static async Task<BlobContainerClient> GetOrCreateCacheContainerAsync(
        this BlobServiceClient blobServiceClient,
        string containerName = DefaultCacheContainerName,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            var container = blobServiceClient.GetBlobContainerClient(containerName);

            //TODO: Takes time - improve this
            await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
            
            if (await container.ExistsAsync(cancellationToken))
            {
                return container;
            }
        }
        catch (RequestFailedException e)
        {
        }
        catch (Exception e)
        {
        }

        return null;
    }
    
    //-------------------------------------------------
    // List blobs in a container
    //-------------------------------------------------
    public static async Task<List<string>?> SearchBlobAsync(
        this BlobServiceClient blobServiceClient,
        string? blobNameSearchPrefix = null,
        string containerName = DefaultCacheContainerName,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var container = await GetOrCreateCacheContainerAsync(
            blobServiceClient, 
            containerName, 
            cancellationToken);
        
        try
        {
            // List all the blobs
            var names = new List<string>();
            await foreach (BlobItem blob in container.GetBlobsAsync(
                               prefix: blobNameSearchPrefix, 
                               cancellationToken: cancellationToken))
            {
                names.Add(blob.Name);
            }

            return names;
        }
        catch (RequestFailedException e)
        {
        }
        catch (Exception e)
        {
        }

        return null;
    }
    
    public static async Task<BlobClient?> GetBlobClientAsync(
        this BlobServiceClient blobServiceClient,
        string blobName,//blobName which is cache key
        string containerName = DefaultCacheContainerName,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var container = await GetOrCreateCacheContainerAsync(
            blobServiceClient, 
            containerName, 
            cancellationToken);
        
        try
        {
            // Get the blob
            BlobClient blob = container.GetBlobClient(blobName);
            if (await blob.ExistsAsync(cancellationToken))
            {
                return blob;
            }
        }
        catch (RequestFailedException e)
        {
        }
        catch (Exception e)
        {
        }

        return null;
    }
    
    
    public static async Task CreateBlobAsync(
        this BlobServiceClient blobServiceClient,
        string blobName,//blobName which is cache key
        byte[] value,
        bool overwrite = true,
        int minutesToCache = 10,
        string containerName = DefaultCacheContainerName,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        var container = await GetOrCreateCacheContainerAsync(
            blobServiceClient, 
            containerName, 
            cancellationToken);
        
        try
        {
            // Get the blob
            BlobClient blob = container.GetBlobClient(blobName);
            
            if (await blob.ExistsAsync(cancellationToken) && !overwrite)
            {
                return;
            }
            else
            {
                // Set the Cache-Control header to cache for 1 day (86400 seconds)
                var blobHttpHeaders = new BlobHttpHeaders
                {
                    CacheControl = $"public, max-age={minutesToCache * 60}"
                };
                
                // Create the blob
                await blob.UploadAsync(BinaryData.FromBytes(value), overwrite, cancellationToken);
                
                // Update the blob's properties to include the new Cache-Control header
                await blob.SetHttpHeadersAsync(blobHttpHeaders,  cancellationToken: cancellationToken);
            }
        }
        catch (Exception e)
        {
            throw;
        }
    }
    
    public static async Task<T?> GetBlobAsync<T>(
        this BlobServiceClient blobServiceClient,
        string blobName, //blobName which is cache key
        string containerName = DefaultCacheContainerName,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            var blobClient = await GetBlobClientAsync(blobServiceClient, blobName, containerName, cancellationToken);
            if (blobClient != null)
            {
                // Create a MemoryStream to store the blob content
                using var memoryStream = new MemoryStream();
                // Download the blob's content to the MemoryStream
                await blobClient.DownloadToAsync(memoryStream, cancellationToken);

                // Set the stream position to the beginning
                memoryStream.Position = 0;

                // Deserialize the JSON content into an object of type T
                return await JsonSerializer.DeserializeAsync<T>(memoryStream, cancellationToken: cancellationToken);
            }
        }
        catch (Exception e)
        {
        }

        return default(T?);
    }
    
    public static async Task<byte[]?> GetBlobAsyncAsByteArray(
        this BlobServiceClient blobServiceClient,
        string blobName, //blobName which is cache key
        string containerName = DefaultCacheContainerName,
        bool invalidateIfExpired = true,
        int minutesToCache = 10,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            var blobClient = await GetBlobClientAsync(blobServiceClient, blobName, containerName, cancellationToken);
            if (blobClient != null)
            {
                if (invalidateIfExpired)
                {
                    var props = await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken);
                    if (props.HasValue)
                    {
                        if ((DateTimeOffset.UtcNow - props.Value.CreatedOn.ToUniversalTime()).Minutes > minutesToCache)
                        {
                            await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                            return default(byte[]?);
                        }
                    }
                }
                
                // Create a MemoryStream to store the blob content
                using var memoryStream = new MemoryStream();
                // Download the blob's content to the MemoryStream
                await blobClient.DownloadToAsync(memoryStream, cancellationToken);

                // Return the byte array
                return memoryStream.ToArray();
            }
        }
        catch (Exception e)
        {
        }

        return default(byte[]?);
    }
    
    public static async Task RemoveBlobAsync(
        this BlobServiceClient blobServiceClient,
        string blobName, //blobName which is cache key
        string containerName = DefaultCacheContainerName,
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            var blobClient = await GetBlobClientAsync(blobServiceClient, blobName, containerName, cancellationToken);
            if (blobClient != null)
            {
                await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
            }
        }
        catch (Exception e)
        {
        }
    }
}
