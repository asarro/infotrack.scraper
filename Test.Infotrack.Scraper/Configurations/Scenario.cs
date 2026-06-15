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

    public Scenario TheSearchSiteRespondsWithListingHtml
    {
        get
        {
            const string html = """
                <html><body>
                  <div class="result-item">
                    <span class="h2">Test Solicitors Ltd<div class="greentick" title="quality marks"></div></span>
                    <div class="phone-block mobile-hidden">
                      <span>Phone:</span>
                      <a rel="noindex" href="tel:02000000000">020 0000 0000</a>
                    </div>
                    <a href="/test-solicitors.html" class="link-map">
                      <address>1 Test Street, London</address>
                    </a>
                    <p>Expert conveyancing solicitors serving London.</p>
                    <ul class="list-item">
                      <li><a target="_blank" href="https://www.test-solicitors.co.uk" rel="nofollow"><i class="fa fa-globe"></i>Website</a></li>
                    </ul>
                  </div>
                </body></html>
                """;
            _mockHttp.When("*").Respond(System.Net.HttpStatusCode.OK,
                new System.Net.Http.StringContent(html, System.Text.Encoding.UTF8, "text/html"));
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
