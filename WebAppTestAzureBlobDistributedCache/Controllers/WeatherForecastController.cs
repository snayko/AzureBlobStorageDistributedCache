using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;

namespace WebAppTestAzureBlobDistributedCache.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

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


        var res = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

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