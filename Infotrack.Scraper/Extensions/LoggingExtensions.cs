using Serilog;

namespace Infotrack.Scraper.Extensions;

internal static class LoggingExtensions
{
    internal static IServiceCollection AddLoggingServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider)
            .Enrich.FromLogContext());

        return services;
    }
}