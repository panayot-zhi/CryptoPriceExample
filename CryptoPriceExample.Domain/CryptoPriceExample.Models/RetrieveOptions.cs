using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoPriceExample.Models
{
    public class RetrieveOptions
    {
        public TimeSpan Interval { get; set; } = TimeSpan.FromSeconds(1);

        public List<string> Symbols { get; set; } = new()
        {
            "BTCUSDT",
            "ADAUSDT",
            "ETHUSDT"
        };
    }
}
