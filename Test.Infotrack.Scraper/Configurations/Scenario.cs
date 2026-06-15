using Infotrack.Scraper.Conveyancing;
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
                    services.AddHttpClient<ISolicitorSearchService, SolicitorSearchService>()
                            .ConfigurePrimaryHttpMessageHandler(() => _mockHttp)));
            return this;
        }
    }

    public Scenario TheSearchSiteRespondsSuccessfully
    {
        get
        {
            _mockHttp.When("*").Respond(System.Net.HttpStatusCode.OK);
            return this;
        }
    }

    public Scenario TheSearchSiteReturnsAnError
    {
        get
        {
            _mockHttp.When("*").Respond(System.Net.HttpStatusCode.InternalServerError);
            return this;
        }
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

    internal HttpClient CreateClient() =>
        (_factory ?? _fixture).CreateClient();
}
