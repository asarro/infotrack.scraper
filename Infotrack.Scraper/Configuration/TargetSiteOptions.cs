namespace Infotrack.Scraper.Configuration;

internal sealed record TargetSiteOptions
{
    public required string Name { get; init; }
    public required string TargetUrl { get; init; }
    public List<string> Locations { get; init; } = [];
    public string? ParsingRulesFile { get; init; }
    public ParsingRules? ParsingRules { get; init; }
}