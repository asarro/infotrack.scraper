using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Conveyancing;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace Infotrack.Scraper.Extensions;

internal static class ConveyancingExtensions
{
    internal static IServiceCollection AddConveyancingServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<List<TargetSiteOptions>>(configuration.GetSection("TargetSites"));
        services.Configure<ScraperRetryOptions>(configuration.GetSection("ScraperRetryPolicy"));
        services.AddSingleton<ILocationProvider, LocationProvider>();

        services.AddHttpClient<ISolicitorSearchService, SolicitorSearchService>((sp, client) =>
            {
                var sites = sp.GetRequiredService<IOptions<List<TargetSiteOptions>>>().Value;
                client.BaseAddress = new Uri(sites[0].TargetUrl);
            })
            .AddResilienceHandler("scraper-retry", (builder, context) =>
            {
                var retry = context.ServiceProvider
                    .GetRequiredService<IOptions<ScraperRetryOptions>>().Value;

                builder.AddRetry(new HttpRetryStrategyOptions
                {
                    MaxRetryAttempts = retry.MaxRetryAttempts,
                    Delay = TimeSpan.FromSeconds(retry.InitialDelaySeconds),
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = false
                });
            });

        return services;
    }
}