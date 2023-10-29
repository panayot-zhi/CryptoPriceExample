namespace CryptoPriceExample.Models
{
    public class SimpleMovingAverageResponse
    {
        public decimal SimpleMovingAverage { get; init; }

        public string Symbol { get; init; }

        public int DataPoints { get; init; }

        public string Period { get; init; }

        public DateTime StartTime { get; init; }
    }
}
