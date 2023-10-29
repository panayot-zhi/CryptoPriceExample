using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CryptoPriceExample.Models
{
    public class RetrieveOptions
    {
        public List<string> Symbols { get; init; } = new();
    }
}
