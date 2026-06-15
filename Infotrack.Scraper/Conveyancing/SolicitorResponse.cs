namespace Infotrack.Scraper.Conveyancing;

/// <summary>
/// Read model returned by the search endpoint. Mirrors <see cref="Solicitor"/> (the
/// parse/write model) but adds <c>CreatedDate</c>, which only exists once a record has
/// been persisted.
/// </summary>
public sealed record SolicitorResponse(
    string Name,
    string? Address,
    string? Phone,
    string? Description,
    string? Website,
    DateTimeOffset CreatedDate);
