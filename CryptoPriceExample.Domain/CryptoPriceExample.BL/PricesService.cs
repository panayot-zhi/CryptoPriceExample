using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Sockets;
using CryptoPriceExample.DAL;
using CryptoPriceExample.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CryptoPriceExample.BL;

public class PricesService
{
    private const string LastRecordedPricePrefix = "LastRecordedPrice_";

    private readonly ILogger<PricesService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMemoryCache _cache;

    public PricesService(ILogger<PricesService> logger, IServiceProvider serviceProvider, IMemoryCache cache)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _cache = cache;
    }

    public void HandleTradeUpdate(DataEvent<BinanceStreamTrade> tradeUpdateEvent)
    {
        // IMPORTANT: we choose to save only prices that changed from the last one, this affects the average price calculation
        // Increasing Significance: Recording only trades with a different price means that each entry in your database has unique value, making every recorded trade potentially more significant.
        // Reducing Redundancy: Since we're avoiding multiple entries with the same price, the data becomes less redundant.
        // but
        // Sensitivity to Price Changes: Since only different prices are recorded, the average becomes more sensitive to actual changes in the market value of the cryptocurrency.
        // Possible Bias: By ignoring repeated trades at the same price, we introduce bias. If a price is stable and repeated across multiple trades, it might be a stronger indicator of the market sentiment at that time. Ignoring these trades could skew the average away from this price.
        
        var lastPrice = GetLastPrice(tradeUpdateEvent.Data.Symbol);
        if (lastPrice == tradeUpdateEvent.Data.Price)
        {
            _logger.LogDebug("Skip trade update, price is the same as last retrieved.");
            return;
        }

        var price = new Price()
        {
            Symbol = tradeUpdateEvent.Data.Symbol,
            Timestamp = tradeUpdateEvent.Data.TradeTime,
            Value = tradeUpdateEvent.Data.Price
        };

        try
        {
            _logger.LogInformation($"Saving trade update for symbol: {tradeUpdateEvent.Data.Symbol}, Price: {tradeUpdateEvent.Data.Price}");

            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Prices.Add(price);
                context.SaveChanges();

                // Refresh cache entry
                _cache.Set(LastRecordedPricePrefix + price.Symbol, 
                    price.Value);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to save price: {Price}", price);
        }
    }

    private decimal? GetLastPrice(string symbol)
    {
        var key = LastRecordedPricePrefix + symbol;
        var lastRecordedPrice = _cache.Get<decimal?>(key);
        if (lastRecordedPrice is not null)
        {
            return lastRecordedPrice;
        }

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var entity = context.Prices
                .Where(x => x.Symbol == symbol)
                .OrderByDescending(x => x.Timestamp)
                .FirstOrDefault();

            if (entity is null)
            {
                return null;
            }

            _cache.Set(key, entity.Value);

            return entity.Value;
        }
    }

    public void HandleTickerUpdate(DataEvent<IBinanceTick> tickerUpdateEvent)
    {
        _logger.LogInformation($"Ticker update for symbol: {tickerUpdateEvent.Data.Symbol}, Price: {tickerUpdateEvent.Data.LastPrice}");
    }

    public async Task<decimal> Calculate24hAvgPrice(string symbol, DateTime startDate)
    {
        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Query the database for prices of the specific symbol within the time periodInSeconds
            var averagePrice = await context.Prices
                .Where(cp => cp.Symbol == symbol && cp.Timestamp >= startDate)
                .AverageAsync(cp => cp.Value);

            return averagePrice;
        }
    }

    public async Task<decimal?> CalculateSimpleMovingAverage(string symbol, int periodInSeconds, int dataPoints, DateTime? startDate = null)
    {
        startDate ??= DateTime.UtcNow.AddSeconds(-dataPoints * periodInSeconds);

        using (var scope = _serviceProvider.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var pricesQuery = context.Prices
                .Where(pr => pr.Symbol == symbol && pr.Timestamp >= startDate)
                .OrderBy(pr => pr.Timestamp);

            var prices = await pricesQuery.ToListAsync();

            if (!prices.Any())
            {
                return null;
            }

            // var simpleMovingAverage = prices
            //     .GroupBy(pr => new
            //         { Interval = (long)(pr.Timestamp.Subtract(startDate).TotalSeconds / intervalInSeconds) })
            //     .OrderBy(g => g.Key.Interval)
            //     .Select(g => new
            //     {
            //         StartTimestamp = startDate.AddSeconds(g.Key.Interval * intervalInSeconds),
            //         SMA = g.Average(pr => pr.Value)
            //     });

            // Calculating the SMA
            var sma = prices
                .GroupBy(cp => Math.Floor((cp.Timestamp - startDate.Value).TotalSeconds / periodInSeconds))
                .Select(g => g.Average(cp => cp.Value))
                .Take(dataPoints)
                .DefaultIfEmpty()
                .Average();

            return sma;
        }
    }

    public static int? ConvertPeriodToSeconds(string period)
    {
        int periodInSeconds;

        switch (period)
        {
            case "1w":
                periodInSeconds = 604800;
                break;
            case "1d":
                periodInSeconds = 86400;
                break;
            case "30m":
                periodInSeconds = 1800;
                break;
            case "5m":
                periodInSeconds = 300;
                break;
            case "1m":
                periodInSeconds = 60;
                break;
            default:
                return null;
        }

        return periodInSeconds;
    }
}