using CSharpFunctionalExtensions;
using Infotrack.Scraper.Models;

namespace Infotrack.Scraper.Conveyancing;

/// <summary>
/// Scrapes a single location from the target site and stores the results in the database.
/// Used by the background worker; the read-only search path no longer scrapes.
/// </summary>
internal interface ISolicitorScrapeService
{
    /// <summary>Returns the number of solicitors stored for the location, or an error.</summary>
    Task<Result<int, Error>> ScrapeAndStoreAsync(string location, CancellationToken cancellationToken = default);
}
