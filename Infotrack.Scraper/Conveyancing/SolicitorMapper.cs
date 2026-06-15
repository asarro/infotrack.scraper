using Infotrack.Scraper.Scraping;

namespace Infotrack.Scraper.Conveyancing;

internal static class SolicitorMapper
{
    internal static Solicitor Map(ExtractedRecord record) => new(
        record.Fields.GetValueOrDefault("Name")        ?? string.Empty,
        record.Fields.GetValueOrDefault("Address"),
        record.Fields.GetValueOrDefault("Phone"),
        record.Fields.GetValueOrDefault("Description"),
        record.Fields.GetValueOrDefault("Website"));
}
