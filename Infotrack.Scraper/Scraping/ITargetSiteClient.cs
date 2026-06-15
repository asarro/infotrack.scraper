using CSharpFunctionalExtensions;
using Infotrack.Scraper.Models;

namespace Infotrack.Scraper.Scraping;

internal interface ITargetSiteClient
{
    Task<Result<string, Error>> FetchHtmlAsync(string location, CancellationToken cancellationToken = default);
}
