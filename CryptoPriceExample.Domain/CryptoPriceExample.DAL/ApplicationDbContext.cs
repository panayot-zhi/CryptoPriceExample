using CryptoPriceExample.Models;
using Microsoft.EntityFrameworkCore;

namespace CryptoPriceExample.DAL
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Price> Prices { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Price>(entity =>
            {
                entity.HasIndex(e => e.Symbol);
                entity.HasIndex(e => e.Timestamp);
            });
        }
    }
}
