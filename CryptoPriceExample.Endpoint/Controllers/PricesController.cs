using System.Security.Cryptography;
using CryptoPriceExample.BL;
using CryptoPriceExample.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CryptoPriceExample.Endpoint.Controllers;

[ApiController]
public class PricesController : ControllerBase
{
    private readonly ILogger<PricesController> _logger;
    private readonly IOptionsMonitor<RetrieveOptions> _options;
    private readonly PricesService _pricesService;

    public PricesController(ILogger<PricesController> logger, IOptionsMonitor<RetrieveOptions> options, PricesService pricesService)
    {
        _logger = logger;
        _options = options;
        _pricesService = pricesService;
    }

    [HttpGet("/api/ping")]
    public IActionResult Ping()
    {
        return Ok("Pong");
    }

    /// <summary>
    /// Returns the average price for the last 24h of data in the database ( or the oldest available, if 24h of data is not available )
    /// </summary>
    /// <param name="symbol">The symbol the average price is being calculated for</param>
    /// <returns></returns>
    [HttpGet("/api/{symbol}/24hAvgPrice")]
    [ResponseCache(CacheProfileName = CacheProfileNames.TenSeconds)]
    public async Task<ActionResult<AveragePriceResponse>> Get24hAvgPrice(string symbol)
    {
        try
        {
            // Check for valid symbol passed
            if (!_options.CurrentValue.Symbols.Contains(symbol))
            {
                return BadRequest("Symbol not supported.");
            }

            // Get the datetime representing 24 hours ago
            var date24HoursAgo = DateTime.Now.AddHours(-24);

            // Calculate the average price from service method
            var averagePrice = await _pricesService.Calculate24hAvgPrice(symbol, date24HoursAgo);

            // Check if any data was retrieved,
            // if not, return a Not Found response
            if (averagePrice == 0)
            {
                return NotFound("No price data available for the last 24 hours.");
            }

            return Ok(new AveragePriceResponse
            {
                StartTime = date24HoursAgo,
                Symbol = symbol,
                Average = averagePrice
            });
        }
        catch (Exception ex)
        {
            var exceptionEvent = new EventId(RandomNumberGenerator.GetInt32(1000, 2000), nameof(Get24hAvgPrice));
            var exceptionMessage = "An error occurred while processing your request. Please contact administrator with this event id: " + exceptionEvent.Id;
            _logger.LogError(exceptionEvent, ex, exceptionMessage);
            return StatusCode(500, exceptionMessage);
        }
    }

    /// <summary>
    /// Return the current Simple Moving average of the symbol's price
    /// </summary>
    /// <param name="symbol">The symbol the average price is being calculated for</param>
    /// <param name="n">The amount of data points</param>
    /// <param name="p">he time period represented by each data point. Acceptable values: `1w`, `1d`, `30m`, `5m`, `1m`</param>
    /// <param name="s">The datetime from which to start the SMA calculation ( a date )</param>
    /// <returns></returns>
    [HttpGet("/api/{symbol}/SimpleMovingAverage")]
    [ResponseCache(CacheProfileName = CacheProfileNames.OneMinute)]
    public async Task<ActionResult<SimpleMovingAverageResponse>> GetSimpleMovingAverage(
        string symbol, int n, string p, DateTime? s)
    {
        try
        {
            if (!_options.CurrentValue.Symbols.Contains(symbol))
            {
                return BadRequest("Symbol not supported.");
            }

            var periodInSeconds = PricesService.ConvertPeriodToSeconds(p);
            if (!periodInSeconds.HasValue)
            {
                return BadRequest("Invalid time period provided.");
            }

            var startDate = s ?? DateTime.UtcNow.AddSeconds(-n * periodInSeconds.Value);

            var sma = await _pricesService.CalculateSimpleMovingAverage(symbol, periodInSeconds.Value, n, startDate);

            if (sma is null)
            {
                return NotFound("No price data available for the specified parameters.");
            }

            if (sma == 0)
            {
                return NotFound("Not enough data to calculate the SMA.");
            }

            return Ok(new SimpleMovingAverageResponse
            {
                Symbol = symbol,
                DataPoints = n,
                Period = p,
                SimpleMovingAverage = (decimal) sma,
                StartTime = startDate
            });
        }
        catch (Exception ex)
        {
            var exceptionEvent = new EventId(RandomNumberGenerator.GetInt32(2000, 3000), nameof(GetSimpleMovingAverage));
            var exceptionMessage = "An error occurred while processing your request. Please contact administrator with this event id: " + exceptionEvent.Id;
            _logger.LogError(exceptionEvent, ex, exceptionMessage);
            return StatusCode(500, exceptionMessage);
        }
    }
}