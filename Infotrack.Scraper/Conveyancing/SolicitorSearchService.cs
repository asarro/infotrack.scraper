using System.Collections.ObjectModel;
using CSharpFunctionalExtensions;
using Infotrack.Scraper.Configuration;
using Infotrack.Scraper.Diagnostics;
using Infotrack.Scraper.Models;
using Infotrack.Scraper.Persistence;
using Infotrack.Scraper.Scraping;
using Microsoft.Extensions.Options;

namespace Infotrack.Scraper.Conveyancing;

internal sealed class SolicitorSearchService(
    ITargetSiteClient siteClient,
    IoMetrics metrics,
    HtmlSanitizer sanitizer,
    HtmlParsingEngine engine,
    IOptions<List<TargetSiteOptions>> siteOptions,
    ISolicitorRepository repository) : ISolicitorSearchService
{
    public async Task<Result<IReadOnlyList<Solicitor>, Error>> SearchAsync(
        string location,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var cached = await repository.GetByLocationAsync(location, cancellationToken);
            if (cached.HasValue)
                return new ReadOnlyCollection<Solicitor>(cached.Value.Select(ToSolicitor).ToList());

            using (metrics.TimeIO("GetSearch"))
            {
                var rules = siteOptions.Value[0].ParsingRules;
                if (rules is null)
                    return new ReadOnlyCollection<Solicitor>([]);

                var fetchResult = await siteClient.FetchHtmlAsync(location, cancellationToken);
                if (fetchResult.IsFailure) return fetchResult.Error;

                var cleanHtml = sanitizer.Sanitize(fetchResult.Value);
                var records   = engine.Parse(cleanHtml, rules);
                var solicitors = records.Select(SolicitorMapper.Map).ToList();

                await repository.UpsertAsync(location, solicitors, cancellationToken);

                return new ReadOnlyCollection<Solicitor>(solicitors);
            }
        }
        catch (Exception ex)
        {
            return new Error(ex.Message);
        }
    }

    private static Solicitor ToSolicitor(SolicitorRecord r) =>
        new(r.Name, r.Address, r.Phone, r.Description, r.Website);
}
