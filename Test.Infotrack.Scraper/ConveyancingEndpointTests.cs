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
    public Task SearchSolicitors_WhenDataStored_ReturnsSolicitors() =>
        When
            .TheApplicationIsRunning
            .And.WithStoredSolicitorsFor("testville")
            .And.SearchForSolicitors("testville")
            .Then.ShouldReturnSolicitors();

    [Fact]
    public Task SearchSolicitors_WhenWarmingUpAndNoData_ReturnsServiceUnavailable() =>
        When
            .TheApplicationIsRunning
            .And.SearchForSolicitors("warming-nowhere")
            .Then.ShouldGetAServiceUnavailableResponse();

    [Fact]
    public Task SearchSolicitors_WhenReadyAndNoData_ReturnsEmptyList() =>
        When
            .TheApplicationIsRunning
            .And.TheScraperHasCompletedAPass
            .And.SearchForSolicitors("empty-nowhere")
            .Then.ShouldReturnAnEmptyList();
}
