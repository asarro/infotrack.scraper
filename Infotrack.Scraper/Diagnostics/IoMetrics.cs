using System.Diagnostics;

namespace Infotrack.Scraper.Diagnostics;

internal sealed class IoMetrics
{
    private readonly Dictionary<string, long> _timings = [];

    public IDisposable TimeIO(string label) => new Timing(this, label);

    internal IReadOnlyDictionary<string, long>? Timings =>
        _timings.Count > 0 ? _timings : null;

    private void Record(string label, long ms) => _timings[label] = ms;

    private sealed class Timing(IoMetrics owner, string label) : IDisposable
    {
        private readonly long _start = Stopwatch.GetTimestamp();

        public void Dispose()
        {
            var elapsed = Stopwatch.GetElapsedTime(_start);
            owner.Record(label, (long)elapsed.TotalMilliseconds);
        }
    }
}
