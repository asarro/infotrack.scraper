namespace Infotrack.Scraper.Models;

/// <summary>
/// Returned by search when a location has no stored records yet and the background
/// scraper has not completed its first pass. The endpoint maps this to HTTP 503 so
/// clients can retry once the data is populated, rather than treating it as a real
/// "no results" answer.
/// </summary>
internal sealed record WarmingUpError() : Error("data_warming_up");
