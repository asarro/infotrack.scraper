using CSharpFunctionalExtensions;
using Infotrack.Scraper.Models;

namespace Infotrack.Scraper.Conveyancing;

internal interface ISolicitorSearchService
{
    Task<Result<IReadOnlyList<Solicitor>, Error>> SearchAsync(string location, CancellationToken cancellationToken = default);
}