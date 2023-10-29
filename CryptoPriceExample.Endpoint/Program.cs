using Binance.Net;
using CryptoPriceExample.BL;
using CryptoPriceExample.DAL;
using CryptoPriceExample.Endpoint.Services;
using CryptoPriceExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CryptoPriceExample.Endpoint;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var configSection = builder.Configuration.GetSection(nameof(RetrieveOptions));
        builder.Services.Configure<RetrieveOptions>(configSection);

        builder.Services.AddHostedService<RetrieveService>();

        // Add services to the container.
        builder.Services.AddControllers(options =>
        {
            options.CacheProfiles.Add(CacheProfileNames.NoCache, new CacheProfile()
            {
                Duration = 0,
                Location = ResponseCacheLocation.None,
                NoStore = true
            });

            options.CacheProfiles.Add(CacheProfileNames.TenSeconds, new CacheProfile()
            {
                VaryByHeader = "Accept",
                Duration = 10 // 10 seconds
            });

            options.CacheProfiles.Add(CacheProfileNames.OneMinute, new CacheProfile()
            {
                VaryByHeader = "Accept",
                Duration = 60 // 1 Minute
            });

            options.CacheProfiles.Add(CacheProfileNames.TenMinutes, new CacheProfile()
            {
                VaryByHeader = "Accept",
                Duration = 10 * 60 // 10 Minutes
            });

            options.CacheProfiles.Add(CacheProfileNames.OneHour, new CacheProfile()
            {
                VaryByHeader = "Accept",
                Duration = 60 * 60 // 1 hour
            });
        })
            .AddXmlSerializerFormatters()
            .AddXmlDataContractSerializerFormatters();

        builder.Services.AddResponseCaching();

        builder.Services.AddServices(builder.Configuration);

        builder.Services.AddBinance(
            restOptions => {
                // set options for the rest client
            },
            socketClientOptions => {
                // set options for the socket client
            });

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