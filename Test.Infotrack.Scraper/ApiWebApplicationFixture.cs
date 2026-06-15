using Infotrack.Scraper.Persistence;
using Infotrack.Scraper.Scraping;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Test.Infotrack.Scraper.Configurations;

namespace Test.Infotrack.Scraper;

public sealed class ApiWebApplicationFixture : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            // Don't let the background scraper run on startup — it would race the test's
            // request and hit the network.
            var worker = services.FirstOrDefault(d => d.ImplementationType == typeof(SolicitorScrapeWorker));
            if (worker is not null)
                services.Remove(worker);

            // Swap the Postgres-backed repository for an in-memory fake so the suite needs
            // no database and never touches the live one.
            services.RemoveAll<ISolicitorRepository>();
            services.AddSingleton<ISolicitorRepository, FakeSolicitorRepository>();
        });
    }
}

[CollectionDefinition(nameof(ApiCollection))]
public class ApiCollection : ICollectionFixture<ApiWebApplicationFixture> { }
