namespace Infotrack.Scraper.Configuration;

internal sealed record FieldRule
{
    public required string Field { get; init; }
    public required string Selector { get; init; }
    public string? Attribute { get; init; }
    public AttributeFilter? AttributeFilter { get; init; }
    public string? StopAt { get; init; }
    public string? ChildSelector { get; init; }
}