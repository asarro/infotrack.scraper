using System.Collections.ObjectModel;
using CSharpFunctionalExtensions;
using Infotrack.Scraper.Diagnostics;
using Infotrack.Scraper.Models;

namespace Infotrack.Scraper.Conveyancing;

internal sealed class SolicitorSearchService(
    HttpClient httpClient,
    IoMetrics metrics) : ISolicitorSearchService
{
    public async Task<Result<IReadOnlyList<Solicitor>, Error>> SearchAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (metrics.TimeIO("GetSearch"))
            {
                var response = await httpClient.GetAsync($"?location={Uri.EscapeDataString(location)}", cancellationToken);

                return response.IsSuccessStatusCode
                    ? new ReadOnlyCollection<Solicitor>([])
                    : new Error($"Target site returned {(int)response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }
}
