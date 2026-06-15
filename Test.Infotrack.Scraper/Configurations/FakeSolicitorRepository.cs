using CSharpFunctionalExtensions;
using Infotrack.Scraper.Conveyancing;
using Infotrack.Scraper.Persistence;

namespace Test.Infotrack.Scraper.Configurations;

internal sealed class FakeSolicitorRepository : ISolicitorRepository
{
    private readonly object _gate = new();
    private long _nextLocationId = 1;
    private readonly Dictionary<string, long> _locationIds = new();
    private readonly Dictionary<string, List<SolicitorRecord>> _byLocation = new();

    public Task<Maybe<IReadOnlyList<SolicitorRecord>>> GetByLocationAsync(
        string location, CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (!_byLocation.TryGetValue(location, out var records))
                return Task.FromResult(Maybe<IReadOnlyList<SolicitorRecord>>.None);

            IReadOnlyList<SolicitorRecord> snapshot = records.ToList();
            return Task.FromResult(Maybe.From(snapshot));
        }
    }

    public Task UpsertAsync(
        string location, IEnumerable<Solicitor> solicitors, CancellationToken ct = default)
    {
        lock (_gate)
        {
            if (!_locationIds.TryGetValue(location, out var locationId))
            {
                locationId = _nextLocationId++;
                _locationIds[location] = locationId;
                _byLocation[location] = [];
            }

            var existing = _byLocation[location];

            foreach (var s in solicitors)
            {
                var key = $"{locationId}:{s.Name}";
                var idx = existing.FindIndex(r => r.SolicitorKey == key);

                if (idx >= 0)
                    // Update mutable fields; created_date stays fixed (mirrors ON CONFLICT DO UPDATE).
                    existing[idx] = existing[idx] with
                    {
                        Address = s.Address,
                        Phone = s.Phone,
                        Description = s.Description,
                        Website = s.Website
                    };
                else
                    existing.Add(new SolicitorRecord(
                        key, locationId, s.Name, s.Address, s.Phone, s.Description, s.Website,
                        DateTime.UtcNow));
            }
        }

        return Task.CompletedTask;
    }
}
