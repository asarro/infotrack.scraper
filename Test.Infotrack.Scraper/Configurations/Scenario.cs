using Infotrack.Scraper.Conveyancing;
using Infotrack.Scraper.Persistence;
using Infotrack.Scraper.Scraping;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RichardSzalay.MockHttp;

namespace Test.Infotrack.Scraper.Configurations;

public sealed class Scenario
{
    private readonly ApiWebApplicationFixture _fixture;
    private WebApplicationFactory<Program>? _factory;
    private readonly MockHttpMessageHandler _mockHttp = new();

    internal string? Location    { get; private set; }
    internal string  Url         { get; private set; } = "/conveyancing/solicitors";

    public Scenario(ApiWebApplicationFixture fixture) => _fixture = fixture;

    public Scenario When => this;
    public Scenario And  => this;

    public Scenario TheApplicationIsRunning
    {
        get
        {
            _factory = _fixture.WithWebHostBuilder(b =>
                b.ConfigureTestServices(services =>
                    services.AddHttpClient<ITargetSiteClient, SolicitorsComClient>()
                            .ConfigurePrimaryHttpMessageHandler(() => _mockHttp)));
            return this;
        }
    }

    /// <summary>Marks the scraper as having finished its first pass (global readiness flag).</summary>
    public Scenario TheScraperHasCompletedAPass
    {
        get
        {
            Factory().Services.GetRequiredService<ScraperReadiness>().MarkReady();
            return this;
        }
    }

    /// <summary>Seeds the database with one solicitor for the given location.</summary>
    public Scenario WithStoredSolicitorsFor(string location)
    {
        using var scope = Factory().Services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ISolicitorRepository>();

        var solicitors = new List<Solicitor>
        {
            new("Seeded Solicitors Ltd", $"1 Test Street, {location}", "020 0000 0000",
                "Expert conveyancing.", "https://example.com")
        };

        repository.UpsertAsync(location.ToLowerInvariant(), solicitors).GetAwaiter().GetResult();
        return this;
    }

    public Scenario IRequestLocations
    {
        get { Url = "/conveyancing/locations"; return this; }
    }

    public Scenario SearchForSolicitors(string location)
    {
        Url      = "/conveyancing/solicitors";
        Location = location;
        return this;
    }

    public Assertions Then => new(this);

    internal HttpClient CreateClient() => Factory().CreateClient();

    private WebApplicationFactory<Program> Factory() => _factory ?? _fixture;
}
