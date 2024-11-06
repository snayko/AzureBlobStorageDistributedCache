namespace WebAppTestAzureBlobDistributedCache;

public class WeatherForecast
{
    public DateTime Date { get; set; }

    public int TemperatureC { get; set; }

    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);

    public string? Summary { get; set; }
    
    public string? BigSummary { get; set; }
    
    public List<WeatherForecast>? HourlyForecast { get; set; }
}