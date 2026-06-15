using CSharpFunctionalExtensions;
using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Diagnostics;
using Infotrack.Scraper.Models;
using Infotrack.Scraper.Persistence;
using Infotrack.Scraper.Scraping;
using Microsoft.Extensions.Options;
using ILogger = Serilog.ILogger;

namespace Infotrack.Scraper.Conveyancing;

/// <summary>
/// Fetches, parses and persists solicitor listings for a single location. This is the
/// scrape pipeline that previously lived inline in <see cref="SolicitorSearchService"/>;
/// it is now driven by the background worker rather than a user request.
/// </summary>
internal sealed class SolicitorScrapeService : ISolicitorScrapeService
{
    private readonly ITargetSiteClient _siteClient;
    private readonly IoMetrics _metrics;
    private readonly HtmlSanitizer _sanitizer;
    private readonly HtmlParsingEngine _engine;
    private readonly IOptions<List<TargetSiteOptions>> _siteOptions;
    private readonly ISolicitorRepository _repository;
    private readonly ILogger _log;

    public SolicitorScrapeService(
        ITargetSiteClient siteClient,
        IoMetrics metrics,
        HtmlSanitizer sanitizer,
        HtmlParsingEngine engine,
        IOptions<List<TargetSiteOptions>> siteOptions,
        ISolicitorRepository repository,
        Serilog.ILogger log)
    {
        _siteClient = siteClient;
        _metrics = metrics;
        _sanitizer = sanitizer;
        _engine = engine;
        _siteOptions = siteOptions;
        _repository = repository;
        _log = log.ForContext<SolicitorScrapeService>();
    }
        
    public async Task<Result<int, Error>> ScrapeAndStoreAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using (_metrics.TimeIO("GetSearch"))
            {
                var rules = _siteOptions.Value[0].ParsingRules;
                if (rules is null)
                    return 0;

                var fetchResult = await _siteClient.FetchHtmlAsync(location, cancellationToken);
                if (fetchResult.IsFailure) return fetchResult.Error;

                var cleanHtml  = _sanitizer.Sanitize(fetchResult.Value);
                var records    = _engine.Parse(cleanHtml, rules);
                var solicitors = records.Select(SolicitorMapper.Map).ToList();

                using (_metrics.TimeIO("UpsertSolicitors"))
                    await _repository.UpsertAsync(location, solicitors, cancellationToken);

                return solicitors.Count;
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }
}
