using CSharpFunctionalExtensions;
using Infotrack.Scraper.Models;

namespace Infotrack.Scraper.Conveyancing;

internal sealed class SolicitorSearchService : ISolicitorSearchService
{
    private readonly HttpClient _httpClient;

    public SolicitorSearchService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<Result<Success, Error>> SearchAsync(string location, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(string.Empty, cancellationToken);

            return response.IsSuccessStatusCode
                ? new Success()
                : new Error($"Target site returned {(int)response.StatusCode}");
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }
}