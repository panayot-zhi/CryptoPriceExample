namespace CryptoPriceExample.Console;

using System;
using BL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Globalization;

internal class Program
{
    // private static readonly HttpClient Client = new();
    // private const string ApiUrl = "http://localhost:5253/api";

    static async Task Main(string[] args)
    {
        Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("en-US");

        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((_, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddServices(context.Configuration);

            }).Build();

        var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger<Program>();

        try
        {
            if (args.Length <= 0)
            {
                logger.LogWarning("No command provided.");
                return;
            }

            string command = args[0].ToLower();

            switch (command)
            {
                case "24h":
                {
                    if (args.Length != 2)
                    {
                        logger.LogWarning("Invalid arguments for '24h' command.");
                        return;
                    }

                    string symbol = args[1];
                    var pricesService = host.Services.GetRequiredService<PricesService>();
                    var date24HoursAgo = DateTime.Now.AddHours(-24);
                    var averagePrice = await pricesService.Calculate24hAvgPrice(symbol, date24HoursAgo);
                    Console.WriteLine("AveragePrice: " + averagePrice);
                    break;
                }

                case "sma":
                {
                    if (args.Length < 4)
                    {
                        logger.LogWarning("Invalid arguments for 'sma' command.");
                        return;
                    }

                    string symbol = args[1];
                    int n = int.Parse(args[2]);
                    string p = args[3];
                    DateTime? s = args.Length == 5 ? DateTime.Parse(args[4]) : null;

                    var pricesService = host.Services.GetRequiredService<PricesService>();
                    var periodInSeconds = PricesService.ConvertPeriodToSeconds(p);
                    if (!periodInSeconds.HasValue)
                    {
                        logger.LogWarning("Invalid value for parameter 'p'.");
                        return;
                    }

                    var sma = await pricesService.CalculateSimpleMovingAverage(symbol, periodInSeconds.Value, n, s);
                    Console.WriteLine("SMA: " + sma);
                    break;
                }

                default:
                    logger.LogWarning("Invalid command.");
                    break;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"An error occurred: {ex.Message}");
        }
    }

    // static async Task<string> Get24hAvgPrice(string symbol)
    // {
    //     var response = await Client.GetAsync($"{ApiUrl}/{symbol}/24hAvgPrice");
    //     response.EnsureSuccessStatusCode();
    //     return await response.Content.ReadAsStringAsync();
    // }
    //
    // static async Task<string> GetSimpleMovingAverage(string symbol, int n, string p, string s)
    // {
    //     string endpoint = $"{ApiUrl}/{symbol}/SimpleMovingAverage?n={n}&p={p}";
    //
    //     if (!string.IsNullOrEmpty(s))
    //     {
    //         endpoint += $"&s={s}";
    //     }
    //
    //     var response = await Client.GetAsync(endpoint);
    //     response.EnsureSuccessStatusCode();
    //     return await response.Content.ReadAsStringAsync();
    // }
}