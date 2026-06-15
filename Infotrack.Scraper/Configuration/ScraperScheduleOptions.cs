namespace Infotrack.Scraper.Configuration;

/// <summary>
/// Configures the background scraper's recurring refresh cadence. Bound from the
/// "ScraperSchedule" configuration section.
/// </summary>
internal sealed class ScraperScheduleOptions
{
    public int IntervalHours { get; set; } = 24;
}
