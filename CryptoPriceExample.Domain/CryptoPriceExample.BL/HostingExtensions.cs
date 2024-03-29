﻿using CryptoPriceExample.DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CryptoPriceExample.BL
{
    public static class HostingExtensions
    {
        public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();
            services.AddTransient<PricesService>();
            services.AddDbContext<ApplicationDbContext>((serviceProvider, dbContextOptions) =>
            {
                var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
                var serverVersion = new MySqlServerVersion(new Version(8, 0, 34));
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (Environment.GetEnvironmentVariable("USE_DATABASE") == "Local")
                {
                    connectionString = connectionString.Replace("mysql", "localhost");
                }

                dbContextOptions
                    .UseMySql(connectionString, serverVersion)
                    .UseLoggerFactory(loggerFactory)
                    .EnableDetailedErrors();
            });

            using (var scope = services.BuildServiceProvider().CreateScope())
            { 
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                context.Database.Migrate();
                context.Database.CloseConnection();
            }

            return services;
        }
    }
}
