using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoPriceExample.Models
{
    public class AveragePriceResponse
    {
        public string Symbol { get; init; }

        public decimal Average { get; init; }

        public DateTime StartTime { get; init; }
    }
}
