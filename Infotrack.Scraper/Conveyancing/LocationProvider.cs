using Infotrack.Scraper.Configuration;
using Microsoft.Extensions.Options;

namespace Infotrack.Scraper.Conveyancing;

internal sealed class LocationProvider : ILocationProvider
{
    private readonly IReadOnlyList<string> _locations;

    public LocationProvider(IOptions<List<TargetSiteOptions>> options)
    {
        _locations = options.Value
            .SelectMany(s => s.Locations)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public IReadOnlyList<string> GetLocations() => _locations;
}