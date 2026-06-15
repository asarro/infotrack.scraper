namespace Infotrack.Scraper.Persistence;

internal sealed record SolicitorRecord(
    string SolicitorKey,
    long LocationId,
    string Name,
    string? Address,
    string? Phone,
    string? Description,
    string? Website);
