using CSharpFunctionalExtensions;
using Infotrack.Scraper.Conveyancing;

namespace Infotrack.Scraper.Persistence;

internal interface ISolicitorRepository
{
    Task<Maybe<IReadOnlyList<SolicitorRecord>>> GetByLocationAsync(string location, CancellationToken ct = default);
    Task UpsertAsync(string location, IEnumerable<Solicitor> solicitors, CancellationToken ct = default);
}
