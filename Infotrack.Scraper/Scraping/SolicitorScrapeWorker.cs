using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Conveyancing;
using Microsoft.Extensions.Options;

namespace Infotrack.Scraper.Scraping;

/// <summary>
/// Collects solicitor records for every configured location into the database — once on
/// startup and then on a recurring interval. Search reads only from the database, so this
/// worker is the sole writer. Locations are scraped sequentially (gentlest on the target
/// site's bot detection); a failure for one location is logged and skipped so the rest
/// still run. Readiness is flipped after each completed pass.
/// </summary>
internal sealed class SolicitorScrapeWorker(
    IServiceScopeFactory scopeFactory,
    ILocationProvider locationProvider,
    ScraperReadiness readiness,
    IOptions<ScraperScheduleOptions> options,
    ILogger<SolicitorScrapeWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var interval = TimeSpan.FromHours(options.Value.IntervalHours);
        using var timer = new PeriodicTimer(interval);

        do
        {
            try
            {
                await RunPassAsync(stoppingToken);
                readiness.MarkReady();
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Scrape pass failed unexpectedly");
            }
        }
        while (await WaitForNextTickAsync(timer, stoppingToken));
    }

    private async Task RunPassAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var scraper = scope.ServiceProvider.GetRequiredService<ISolicitorScrapeService>();

        foreach (var location in locationProvider.GetLocations())
        {
            ct.ThrowIfCancellationRequested();

            // Search stores/looks up locations lower-cased (the endpoint lower-cases the
            // query); keep the worker consistent or lookups will never match.
            var normalized = location.ToLowerInvariant();

            try
            {
                var result = await scraper.ScrapeAndStoreAsync(normalized, ct);
                if (result.IsFailure)
                    logger.LogWarning("Scrape failed for {Location}: {Error}", normalized, result.Error.Message);
                else
                    logger.LogInformation("Scraped {Count} solicitors for {Location}", result.Value, normalized);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error scraping {Location}", normalized);
            }
        }
    }

    private static async Task<bool> WaitForNextTickAsync(PeriodicTimer timer, CancellationToken ct)
    {
        try
        {
            return await timer.WaitForNextTickAsync(ct);
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }
}
