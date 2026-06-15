namespace Infotrack.Scraper.Configuration;

internal sealed record ScraperRetryOptions
{
    public int MaxRetryAttempts { get; init; } = 1;
    public int InitialDelaySeconds { get; init; } = 5;
}