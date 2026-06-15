namespace Infotrack.Scraper.Models;

internal record Error(string Message);

internal sealed record BotDetectionError()
    : Error($"bot_protection_triggered");