using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using WebAppTestAzureBlobDistributedCache.Extensions;

namespace WebAppTestAzureBlobDistributedCache.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IDistributedCache _cache;
    
    public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache cache)
    {
        _logger = logger;
        _cache = cache;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public async Task<IEnumerable<WeatherForecast>> Get(string? userId = null, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var cachedRes = await _cache.GetAsync($"{userId}-weather", cancellationToken);
            if (cachedRes != null)
            {
                return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(
                    Encoding.UTF8.GetString(cachedRes));
            }
        }
        
        Random random = new Random();
        
        //From 40 to 700Kb response
        int randomResponseSizeToGenerate = random.Next(40, 701);

        var res = FakeDataGenerator.GenerateRandomWeatherForecastJson(randomResponseSizeToGenerate);
        
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var options = new DistributedCacheEntryOptions();
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var json = JsonSerializer.Serialize(res);
            var bytes = Encoding.UTF8.GetBytes(json);
            await _cache.SetAsync($"{userId}-weather", bytes, options, cancellationToken);
        }
        
        return res;
    }
}