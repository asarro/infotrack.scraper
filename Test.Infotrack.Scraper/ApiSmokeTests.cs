using System.Net;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Test.Infotrack.Scraper;

// Confirms the WebApplicationFactory plumbing works end-to-end: the API boots and
// serves its OpenAPI document. OpenAPI needs no database, so this stays green in CI
// without a Postgres container. Endpoints that touch the database get their own
// integration tests once domain logic exists.
[Collection(nameof(ApiCollection))]
public class ApiSmokeTests
{
    private readonly ApiWebApplicationFixture _fixture;

    public ApiSmokeTests(ApiWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task OpenApi_Document_Is_Served()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync("/openapi/v1.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // Guards SolicitorScrapeService's constructor injection of Serilog.ILogger: the worker is
    // removed from the test host so the scrape service is never resolved there, making this
    // the only check that the Serilog.ILogger registration is wired up correctly.
    [Fact]
    public void Serilog_ILogger_Is_Resolvable()
    {
        _fixture.Services.GetService<Serilog.ILogger>().Should().NotBeNull();
    }
}
