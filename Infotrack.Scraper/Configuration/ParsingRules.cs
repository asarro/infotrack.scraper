namespace Infotrack.Scraper.Configuration;

internal sealed record ParsingRules
{
    public required string ContainerSelector { get; init; }
    public List<FieldRule> Fields { get; init; } = [];
}