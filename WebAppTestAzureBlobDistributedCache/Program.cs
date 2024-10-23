using Azure.Identity;
using Microsoft.Extensions.Azure;
using BlobStorageDistributedCache;

namespace WebAppTestAzureBlobDistributedCache;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAzureBlobDistributedCache();

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
}