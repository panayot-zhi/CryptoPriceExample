using Microsoft.AspNetCore.Mvc;

namespace CryptoPriceExample.Endpoint.Controllers;

[ApiController]
public class PricesController : ControllerBase
{
    private readonly ILogger<PricesController> _logger;

    public PricesController(ILogger<PricesController> logger)
    {
        _logger = logger;
    }

    [HttpGet("ping")]
    public async Task<IActionResult> Ping()
    {
        return Ok("Pong");
    }

    [HttpGet("{symbol}/24hAvgPrice")]
    public async Task<ActionResult<decimal>> Get24hAvgPrice(string symbol)
    {
        // Implementation goes here.
        // Query the database, calculate the average, and return.
        throw new NotImplementedException();
    }

    [HttpGet("{symbol}/SimpleMovingAverage")]
    public async Task<IActionResult> GetSimpleMovingAverage(
        string symbol, int n, string p, DateTime? s)
    {
        int intervalInSeconds;

        switch (p)
        {
            case "1w":
                intervalInSeconds = 604800;
                break;
            case "1d":
                intervalInSeconds = 86400;
                break;
            case "30m":
                intervalInSeconds = 1800;
                break;
            case "5m":
                intervalInSeconds = 300;
                break;
            case "1m":
                intervalInSeconds = 60;
                break;
            default:
                return BadRequest("Invalid time period provided");
        }

        var startDate = s ?? DateTime.UtcNow.AddSeconds(-n * intervalInSeconds);

        throw new NotImplementedException();
    }
}