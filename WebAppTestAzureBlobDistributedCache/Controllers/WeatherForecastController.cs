using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using WebAppTestAzureBlobDistributedCache.Extensions;

using System.Diagnostics;


namespace WebAppTestAzureBlobDistributedCache.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly IDistributedCache _cache;
    
    private static List<double> _fakeDataGenerationTimes = new List<double>();
    
    private static List<double> _cacheGetDataTimes = new List<double>();
    
    private static List<double> _cacheSaveDataTimes = new List<double>();
    
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
            var stopwatchGetCache = Stopwatch.StartNew();

            var cachedRes = await _cache.GetAsync($"{userId}-weather", cancellationToken);

            stopwatchGetCache.Stop();

            _cacheGetDataTimes.Add(stopwatchGetCache.Elapsed.TotalSeconds);

            if (cachedRes != null)
            {
                return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(
                    Encoding.UTF8.GetString(cachedRes));
            }
        }

        var stopwatchGenerateJson = Stopwatch.StartNew();
        
        Random random = new Random();
        
        //From 40 to 700Kb response
        int randomResponseSizeToGenerate = random.Next(40, 701);

        var res = FakeDataGenerator.GenerateRandomWeatherForecastJson(randomResponseSizeToGenerate);
        
        stopwatchGenerateJson.Stop();
        
        _fakeDataGenerationTimes.Add(stopwatchGenerateJson.Elapsed.TotalSeconds);

        if (!string.IsNullOrWhiteSpace(userId))
        {
            var options = new DistributedCacheEntryOptions();
            options.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

            var json = JsonSerializer.Serialize(res);
            var bytes = Encoding.UTF8.GetBytes(json);
            
            var stopwatchPostCache = Stopwatch.StartNew();
            
            await _cache.SetAsync($"{userId}-weather", bytes, options, cancellationToken);
            
            stopwatchPostCache.Stop();

            _cacheSaveDataTimes.Add(stopwatchPostCache.Elapsed.TotalSeconds);
        }
        
        LogAverageTime();
        
        return res;
    }
    
    private void LogAverageTime()
    {
        if (_fakeDataGenerationTimes.Count > 0)
        {
            var averageTime = _fakeDataGenerationTimes.Average();
            _logger.LogInformation($"Average fake data generation time: {averageTime} seconds");
        }
        
        if (_cacheGetDataTimes.Count > 0)
        {
            var averageTime = _cacheGetDataTimes.Average();
            _logger.LogInformation($"Average GET cache time: {averageTime} seconds");
        }
        
        if (_cacheSaveDataTimes.Count > 0)
        {
            var averageTime = _cacheSaveDataTimes.Average();
            _logger.LogInformation($"Average SAVE cache time: {averageTime} seconds");
        }
    }
}