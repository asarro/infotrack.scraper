using System.Net;
using AwesomeAssertions;

namespace Test.Infotrack.Scraper.Configurations;

public sealed class Assertions(Scenario scenario)
{
    public async Task ShouldGetASuccessfulResponse()
    {
        var response = await scenario.CreateClient().GetAsync(BuildUrl());
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    public async Task ShouldGetABadRequestResponse()
    {
        var response = await scenario.CreateClient().GetAsync(BuildUrl());
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }


    private string BuildUrl() =>
        scenario.Location is null
            ? scenario.Url
            : $"{scenario.Url}?location={Uri.EscapeDataString(scenario.Location)}";
}
