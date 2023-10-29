using Binance.Net.Interfaces;
using Binance.Net.Objects.Models.Spot.Socket;
using CryptoExchange.Net.Sockets;
using CryptoPriceExample.DAL;
using CryptoPriceExample.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CryptoPriceExample.BL;

public class PricesService
{
    private readonly ILogger<PricesService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public PricesService(ILogger<PricesService> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public void HandleTradeUpdate(DataEvent<BinanceStreamTrade> tradeUpdateEvent)
    {
        _logger.LogInformation($"Trade update for symbol: {tradeUpdateEvent.Data.Symbol}, Price: {tradeUpdateEvent.Data.Price}");

        var price = new Price()
        {
            Symbol = tradeUpdateEvent.Data.Symbol,
            Timestamp = tradeUpdateEvent.Data.TradeTime,
            Value = tradeUpdateEvent.Data.Price
        };

        try
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // var entity = context.Prices
                //     .Where(x => x.Symbol == tradeUpdateEvent.Data.Symbol)
                //     .OrderByDescending(x => x.Timestamp)
                //     .FirstOrDefault();

                context.Prices.Add(price);
                context.SaveChanges();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while trying to save price: {Price}", price);
        }
    }

    public void HandleTickerUpdate(DataEvent<IBinanceTick> tickerUpdateEvent)
    {
        _logger.LogInformation($"Ticker update for symbol: {tickerUpdateEvent.Data.Symbol}, Price: {tickerUpdateEvent.Data.LastPrice}");
    }

}