using Binance.Net;
using CryptoPriceExample.BL;
using CryptoPriceExample.DAL;
using CryptoPriceExample.Endpoint.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace CryptoPriceExample.Endpoint;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();

        builder.Services.AddTransient<PricesService>();

        builder.Services.AddBinance(
            restOptions => {
                // set options for the rest client
            },
            socketClientOptions => {
                // set options for the socket client
            });

        // Replace with your connection string.
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

        // Replace with your server version and type.
        // Use 'MariaDbServerVersion' for MariaDB.
        // Alternatively, use 'ServerVersion.AutoDetect(connectionString)'.
        // For common usages, see pull request #1233.
        var serverVersion = new MySqlServerVersion(new Version(8, 0, 34));

        // Replace 'ApplicationDbContext' with the name of your own DbContext derived class.
        builder.Services.AddDbContext<ApplicationDbContext>(
            dbContextOptions => dbContextOptions
                .UseMySql(connectionString, serverVersion)
                // The following three options help with debugging, but should
                // be changed or removed for production.
                .LogTo(Console.WriteLine, LogLevel.Information)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
        );

        builder.Services.AddHostedService<RetrieveService>();

        // Learn more about configuring Swagger/OpenAPI
        // at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

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