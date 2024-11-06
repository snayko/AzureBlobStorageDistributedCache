using System.Text;
using System.Text.Json;
using Bogus;

namespace WebAppTestAzureBlobDistributedCache.Extensions;

public static class FakeDataGenerator
{
    // Generate random JSON of specific size
    public static IEnumerable<WeatherForecast>? GenerateRandomWeatherForecastJson(int sizeInKb)
    {
        var currentDate = DateTime.UtcNow;
        
        // Create a Faker to generate random instances of ExampleData
        var faker = new Faker<WeatherForecast>()
            .RuleFor(e => e.Date, f => currentDate)
            .RuleFor(e => e.TemperatureC, f => f.Random.Number(-30, 55))
            .RuleFor(e => e.Summary, f => f.Lorem.Paragraph())
            .RuleFor(e => e.BigSummary, f => f.Lorem.Paragraphs(10))
            .RuleFor(e => e.HourlyForecast, f => f.Make(5, () => new WeatherForecast
            {
                Date = currentDate.AddHours(1),
                TemperatureC = f.Random.Number(-30, 55),
                Summary = f.Lorem.Paragraph()
            }));

        var json = new StringBuilder();
        
        // Continuously add serialized objects until we reach the desired size
        while (Encoding.UTF8.GetByteCount(json.ToString()) < sizeInKb * 1024)
        {
            // Serialize the object using System.Text.Json
            var exampleDataJson = JsonSerializer.Serialize(faker.Generate());
            json.Append(exampleDataJson).Append(",");
            
            currentDate = currentDate.AddDays(1);
        }
        
        // Wrap it in array brackets for valid JSON format
        var strResult = $"[{json.ToString().TrimEnd(',')}]";

        return JsonSerializer.Deserialize<IEnumerable<WeatherForecast>>(strResult);
    }
}