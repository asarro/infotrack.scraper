using System.Text.Json;
using Infotrack.Scraper.Configuration;
using Microsoft.Extensions.Options;

namespace Infotrack.Scraper.Utils;

internal sealed class ParsingRulesFileLoader(IHostEnvironment env)
    : IPostConfigureOptions<List<TargetSiteOptions>>
{
    public void PostConfigure(string? name, List<TargetSiteOptions> options)
    {
        for (var i = 0; i < options.Count; i++)
        {
            var site = options[i];
            if (site.ParsingRulesFile is null) continue;

            var path = Path.Combine(env.ContentRootPath, "Resources", site.ParsingRulesFile);
            if (!File.Exists(path)) continue;

            var rules = JsonSerializer.Deserialize<ParsingRules>(File.ReadAllText(path));
            if (rules is not null)
                options[i] = site with { ParsingRules = rules };
        }
    }
}
