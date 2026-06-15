using CSharpFunctionalExtensions;
using Infotrack.Scraper.Models;

namespace Infotrack.Scraper.Scraping;

internal sealed class SolicitorsComClient(HttpClient httpClient) : ITargetSiteClient
{
    public async Task<Result<string, Error>> FetchHtmlAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            $"{httpClient.BaseAddress}+{Uri.EscapeDataString(location)}.html",
            cancellationToken);

        if (!response.IsSuccessStatusCode)
            return new Error($"Target site returned {(int)response.StatusCode}");

        var rawHtml = await response.Content.ReadAsStringAsync(cancellationToken);

        return rawHtml.Trim() == "NX2"
            ? new BotDetectionError()
            : Result.Success<string, Error>(rawHtml);
    }
}
