using CSharpFunctionalExtensions;
using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Diagnostics;
using Infotrack.Scraper.Models;
using Infotrack.Scraper.Persistence;
using Infotrack.Scraper.Scraping;
using Microsoft.Extensions.Options;

namespace Infotrack.Scraper.Conveyancing;

/// <summary>
/// Fetches, parses and persists solicitor listings for a single location. This is the
/// scrape pipeline that previously lived inline in <see cref="SolicitorSearchService"/>;
/// it is now driven by the background worker rather than a user request.
/// </summary>
internal sealed class SolicitorScrapeService(
    ITargetSiteClient siteClient,
    IoMetrics metrics,
    HtmlSanitizer sanitizer,
    HtmlParsingEngine engine,
    IOptions<List<TargetSiteOptions>> siteOptions,
    ISolicitorRepository repository) : ISolicitorScrapeService
{
    public async Task<Result<int, Error>> ScrapeAndStoreAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        using (metrics.TimeIO("GetSearch"))
        {
            var rules = siteOptions.Value[0].ParsingRules;
            if (rules is null)
                return 0;

            var fetchResult = await siteClient.FetchHtmlAsync(location, cancellationToken);
            if (fetchResult.IsFailure) return fetchResult.Error;

            var cleanHtml  = sanitizer.Sanitize(fetchResult.Value);
            var records    = engine.Parse(cleanHtml, rules);
            var solicitors = records.Select(SolicitorMapper.Map).ToList();

            await repository.UpsertAsync(location, solicitors, cancellationToken);

            return solicitors.Count;
        }
    }
}
