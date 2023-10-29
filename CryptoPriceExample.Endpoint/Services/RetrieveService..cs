using CryptoPriceExample.Models;
using Microsoft.Extensions.Options;
using Binance.Net.Interfaces.Clients;
using CryptoPriceExample.BL;

namespace CryptoPriceExample.Endpoint.Services
{
    public class RetrieveService : BackgroundService
    {
        private readonly ILogger<RetrieveService> _logger;
        private readonly IOptionsMonitor<RetrieveOptions> _options;
        private readonly IBinanceSocketClient _socketClient;
        private readonly PricesService _pricesService;

        public RetrieveService(ILogger<RetrieveService> logger, IOptionsMonitor<RetrieveOptions> options,
            IBinanceSocketClient socketClient, PricesService pricesService)
        {
            _logger = logger;
            _options = options;
            _socketClient = socketClient;
            _pricesService = pricesService;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Subscribe to symbols: {@Symbols}", _options.CurrentValue.Symbols);

            // var exchangeInfo = await _socketClient.SpotApi.ExchangeData.GetExchangeInfoAsync(_options.CurrentValue.Symbols);
            // _logger.LogInformation("ExchangeInfo for symbols: {@ExchangeInfo}",
            //     exchangeInfo);

            // NOTE: A reasonable alternative here would be to obtain last trades for symbol over a certain interval and store them.
            // var recentTrades = await _socketClient.SpotApi.ExchangeData.GetRecentTradesAsync(_options.CurrentValue.Symbols.First());
            // _logger.LogInformation("RecentTrades for symbol ({Symbol}): {@ExchangeInfo}",
            //     _options.CurrentValue.Symbols.First(), recentTrades);
            
            // _logger.LogInformation("Subscribing to Spot.TickerUpdates for the following symbols: {@Symbols}", 
            //     _options.CurrentValue.Symbols);
            //
            // await _socketClient.SpotApi.ExchangeData.SubscribeToTickerUpdatesAsync(_options.CurrentValue.Symbols,
            //     _pricesService.HandleTickerUpdate, stoppingToken);

            _logger.LogInformation("Subscribing to Spot.TradeUpdates for the following symbols: {@Symbols}",
                _options.CurrentValue.Symbols);

            await _socketClient.SpotApi.ExchangeData.SubscribeToTradeUpdatesAsync(_options.CurrentValue.Symbols,
                _pricesService.HandleTradeUpdate, cancellationToken);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Cancellation requested. Unsubscribing and exiting...");
            await _socketClient.UnsubscribeAllAsync();
            _socketClient.Dispose();
        }
    }
}
