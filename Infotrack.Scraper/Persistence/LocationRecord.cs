namespace Infotrack.Scraper.Persistence;

internal sealed record LocationRecord(
    long Id,
    string Name,
    DateTimeOffset LastUpdated);
