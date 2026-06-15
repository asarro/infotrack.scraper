namespace Infotrack.Scraper.Scraping;

/// <summary>
/// Tracks whether the background scraper has completed at least one full pass over all
/// locations. Registered as a singleton: the worker flips it, the search service reads it
/// to tell "still warming up" apart from "genuinely no results".
/// </summary>
internal sealed class ScraperReadiness
{
    private volatile bool _isReady;

    public bool IsReady => _isReady;

    public void MarkReady() => _isReady = true;
}
