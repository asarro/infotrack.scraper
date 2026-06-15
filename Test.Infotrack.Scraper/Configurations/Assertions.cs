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


    public async Task ShouldReturnSolicitors()
    {
        var response = await scenario.CreateClient().GetAsync(BuildUrl());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var docs = System.Text.Json.JsonDocument.Parse(json);
        var arr = docs.RootElement;

        arr.ValueKind.Should().Be(System.Text.Json.JsonValueKind.Array);
        arr.GetArrayLength().Should().BeGreaterThan(0);
        arr[0].GetProperty("name").GetString().Should().NotBeNullOrEmpty();
    }

    private string BuildUrl() =>
        scenario.Location is null
            ? scenario.Url
            : $"{scenario.Url}?location={Uri.EscapeDataString(scenario.Location)}";
}