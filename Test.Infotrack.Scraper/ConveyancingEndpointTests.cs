using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;

namespace Test.Infotrack.Scraper;

[Collection(nameof(ApiCollection))]
public class ConveyancingEndpointTests
{
    private readonly ApiWebApplicationFixture _fixture;

    public ConveyancingEndpointTests(ApiWebApplicationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Locations_Returns_Ok_With_Non_Empty_List()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync("/conveyancing/locations");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var locations = await response.Content.ReadFromJsonAsync<List<string>>();
        locations.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Solicitors_Returns_Ok_For_Location()
    {
        var client = _fixture.CreateClient();

        var response = await client.GetAsync("/conveyancing/solicitors?location=London");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
