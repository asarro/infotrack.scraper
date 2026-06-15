using Test.Infotrack.Scraper.Configurations;

namespace Test.Infotrack.Scraper;

[Collection(nameof(ApiCollection))]
public class ConveyancingEndpointTests(ApiWebApplicationFixture fixture)
{
    private Scenario When => new(fixture);

    [Fact]
    public Task Locations_ReturnsOk() =>
        When
            .TheApplicationIsRunning
            .And.IRequestLocations
            .Then.ShouldGetASuccessfulResponse();

    [Fact]
    public Task SearchSolicitors_WhenSiteRespondsSuccessfully_ReturnsOk() =>
        When
            .TheApplicationIsRunning
            .And.TheSearchSiteRespondsSuccessfully
            .And.SearchForSolicitors("London")
            .Then.ShouldGetASuccessfulResponse();

    [Fact]
    public Task SearchSolicitors_WhenSiteReturnsError_ReturnsBadRequest() =>
        When
            .TheApplicationIsRunning
            .And.TheSearchSiteReturnsAnError
            .And.SearchForSolicitors("London")
            .Then.ShouldGetABadRequestResponse();

    [Fact]
    public Task SearchSolicitors_WhenSiteRespondsWithHtml_ReturnsSolicitors() =>
        When
            .TheApplicationIsRunning
            .And.TheSearchSiteRespondsWithListingHtml
            .And.SearchForSolicitors("London")
            .Then.ShouldReturnSolicitors();
}
