using System.Diagnostics;
using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Conveyancing;
using Infotrack.Scraper.Diagnostics;
using Microsoft.Extensions.Options;

namespace Infotrack.Scraper.Scraping;

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
        var scopeState = new Dictionary<string, object>
        {
            ["ScrapeRunId"] = Guid.NewGuid().ToString()
        };

        using (logger.BeginScope(scopeState))
        {
            var startedAt = Stopwatch.GetTimestamp();
            var locations = locationProvider.GetLocations();
            var succeeded = 0;
            var failed = 0;
            var stored = 0;

            foreach (var location in locations)
            {
                ct.ThrowIfCancellationRequested();
                var normalized = location.ToLowerInvariant();

                try
                {
                    using var scope = scopeFactory.CreateScope();
                    var scraper = scope.ServiceProvider.GetRequiredService<ISolicitorScrapeService>();
                    var metrics = scope.ServiceProvider.GetRequiredService<IoMetrics>();

                    var result = await scraper.ScrapeAndStoreAsync(normalized, ct);
                    if (result.IsFailure)
                    {
                        failed++;
                        logger.LogWarning("Scrape failed for {Location}: {Error} {@Timings}",
                            normalized, result.Error.Message, metrics.Timings);
                    }
                    else
                    {
                        succeeded++;
                        stored += result.Value;
                        logger.LogInformation("Scraped {Count} solicitors for {Location} {@Timings}",
                            result.Value, normalized, metrics.Timings);
                    }
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    failed++;
                    logger.LogError(ex, "Unexpected error scraping {Location}", normalized);
                }
            }

            var elapsedMs = Stopwatch.GetElapsedTime(startedAt).TotalMilliseconds;
            WriteLog(failed, succeeded, locations, stored, elapsedMs);
        }
    }

    private void WriteLog(int failed, int succeeded, IReadOnlyList<string> locations, int stored, double elapsedMs)
    {
        if (failed > 0)
        {
            logger.LogWarning(
                "Scrape run completed: {Succeeded}/{LocationCount} locations succeeded, "
                + "{Stored} solicitors stored, {Failed} failed in {Elapsed:0.0000} ms",
                succeeded, locations.Count, stored, failed, elapsedMs);
        }
        else
        {
            logger.LogInformation(
                "Scrape run completed: {Succeeded}/{LocationCount} locations succeeded, "
                + "{Stored} solicitors stored, {Failed} failed in {Elapsed:0.0000} ms",
                succeeded, locations.Count, stored, failed, elapsedMs);
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