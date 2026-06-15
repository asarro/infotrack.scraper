using System.Net;
using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Conveyancing;
using Infotrack.Scraper.Scraping;
using Infotrack.Scraper.Utils;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using Polly;

namespace Infotrack.Scraper.Extensions;

internal static class ConveyancingExtensions
{
    internal static IServiceCollection AddServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<List<TargetSiteOptions>>(configuration.GetSection("TargetSites"))
            .AddSingleton<IPostConfigureOptions<List<TargetSiteOptions>>, ParsingRulesFileLoader>()
            .Configure<ScraperRetryOptions>(configuration.GetSection("ScraperRetryPolicy"))
            .AddSingleton<ILocationProvider, LocationProvider>()
            .AddSingleton<HtmlSanitizer>()
            .AddSingleton<HtmlParsingEngine>()
            .AddScoped<ISolicitorSearchService, SolicitorSearchService>()
            .AddHttpClients();

        return services;
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection services)
    {
        services.AddHttpClient<ITargetSiteClient, SolicitorsComClient>((sp, client) =>
            {
                var sites = sp.GetRequiredService<IOptions<List<TargetSiteOptions>>>().Value;
                client.BaseAddress = new Uri(sites[0].TargetUrl);
                client.DefaultRequestHeaders.Add("User-Agent",
                    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36");
                client.DefaultRequestHeaders.Add("Accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-GB,en;q=0.9");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            {
                AutomaticDecompression = DecompressionMethods.All
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
                    UseJitter = false,
                    ShouldHandle = args => args.Outcome switch
                    {
                        { Exception: not null } => PredicateResult.True(),
                        { Result.IsSuccessStatusCode: false } when
                            (int)args.Outcome.Result!.StatusCode >= 500 => PredicateResult.True(),
                        _ => PredicateResult.False()
                    }
                });
            });

        return services;
    }
}