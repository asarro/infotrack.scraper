using System.Collections.ObjectModel;
using CSharpFunctionalExtensions;
using Infotrack.Scraper.Diagnostics;
using Infotrack.Scraper.Models;
using Infotrack.Scraper.Persistence;
using Infotrack.Scraper.Scraping;

namespace Infotrack.Scraper.Conveyancing;

/// <summary>
/// Read-only search: returns solicitor records for a location straight from the database.
/// Scraping is owned by <see cref="SolicitorScrapeWorker"/>. When a location has no stored
/// records and the scraper has not yet completed its first pass, a <see cref="WarmingUpError"/>
/// is returned so the endpoint can answer 503 (retry shortly) rather than a false "no results".
/// </summary>
internal sealed class SolicitorSearchService(
    ISolicitorRepository repository,
    ScraperReadiness readiness,
    IoMetrics metrics) : ISolicitorSearchService
{
    public async Task<Result<IReadOnlyList<SolicitorResponse>, Error>> SearchAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Maybe<IReadOnlyList<SolicitorRecord>> stored;
            using (metrics.TimeIO("GetByLocation"))
                stored = await repository.GetByLocationAsync(location, cancellationToken);
            if (stored.HasValue)
                return new ReadOnlyCollection<SolicitorResponse>(stored.Value.Select(ToResponse).ToList());

            // No rows for this location: warming up if the first pass hasn't finished,
            // otherwise a genuine empty result.
            if (!readiness.IsReady)
                return new WarmingUpError();

            return new ReadOnlyCollection<SolicitorResponse>([]);
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    private static SolicitorResponse ToResponse(SolicitorRecord r) =>
        new(r.Name, r.Address, r.Phone, r.Description, r.Website,
            new DateTimeOffset(r.CreatedDate, TimeSpan.Zero));
}
