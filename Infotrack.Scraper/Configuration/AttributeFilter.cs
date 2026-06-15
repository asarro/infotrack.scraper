namespace Infotrack.Scraper.Configuration;

internal sealed record AttributeFilter
{
    public required string Name  { get; init; }
    public required string Value { get; init; }
}
