using Azure.Identity;
using Microsoft.Extensions.Azure;
using BlobStorageDistributedCache;
using Serilog;
using Serilog.Events;
using WebAppTestAzureBlobDistributedCache.Extensions;

namespace WebAppTestAzureBlobDistributedCache;

public class Program
{
    public static void Main(string[] args)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateBootstrapLogger();
        
        try
        {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            //builder.Services.AddAzureBlobDistributedCache();

            builder.Services.AddAzureRedisDistributedCache(
                builder.Configuration.GetValue<string>("RedisConnectionString"));

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddSecretClient(
                    builder.Configuration.GetSection("KeyVault"));

                clientBuilder.AddBlobServiceClient(builder.Configuration.GetSection("Storage:ConnectionString"));

                clientBuilder.UseCredential(new DefaultAzureCredential());

                // Set up any default settings
                clientBuilder.ConfigureDefaults(
                    builder.Configuration.GetSection("AzureDefaults"));
            });

            builder.Services.AddSerilog((services, lc) => lc
                .ReadFrom.Configuration(builder.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .WriteTo.Console());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();

        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }
}