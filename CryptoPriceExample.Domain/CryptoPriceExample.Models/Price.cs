using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace CryptoPriceExample.Models;

public class Price
{
    public int Id { get; set; }

    [Column(TypeName = "varchar(9)")]
    public string Symbol { get; set; }
    
    [Column(TypeName = "decimal(16, 4)")]
    public decimal Value { get; set; }

    public DateTime Timestamp { get; set; }
}