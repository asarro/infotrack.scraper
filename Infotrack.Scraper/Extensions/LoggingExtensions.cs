using Infotrack.Scraper.Diagnostics;
using Serilog.Events;

namespace Infotrack.Scraper.Extensions;

internal static class LoggingExtensions
{
    internal static IServiceCollection AddLoggingServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IoMetrics>();

        services.AddSerilog((serviceProvider, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(serviceProvider)
            .Enrich.FromLogContext());

        return services;
    }

    internal static WebApplication UseRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.GetLevel = (context, _, _) =>
                context.Request.Path.StartsWithSegments("/health")
                    ? LogEventLevel.Verbose
                    : LogEventLevel.Information;

            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var timings = httpContext.RequestServices.GetService<IoMetrics>()?.Timings;
                if (timings is not null)
                    diagnosticContext.Set("Timings", timings, destructureObjects: true);
            };
        });

        return app;
    }
}