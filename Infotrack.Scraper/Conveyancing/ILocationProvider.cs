namespace Infotrack.Scraper.Conveyancing;

internal interface ILocationProvider
{
    IReadOnlyList<string> GetLocations();
}