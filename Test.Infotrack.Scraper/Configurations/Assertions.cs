using System.Net;
using System.Text.Json;
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

    public async Task ShouldGetAServiceUnavailableResponse()
    {
        var response = await scenario.CreateClient().GetAsync(BuildUrl());
        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    public async Task ShouldReturnSolicitors()
    {
        var response = await scenario.CreateClient().GetAsync(BuildUrl());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var arr = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        arr.ValueKind.Should().Be(JsonValueKind.Array);
        arr.GetArrayLength().Should().BeGreaterThan(0);
        arr[0].GetProperty("name").GetString().Should().NotBeNullOrEmpty();
        arr[0].GetProperty("createdDate").GetDateTimeOffset().Should().NotBe(default);
    }

    public async Task ShouldReturnAnEmptyList()
    {
        var response = await scenario.CreateClient().GetAsync(BuildUrl());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var arr = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

        arr.ValueKind.Should().Be(JsonValueKind.Array);
        arr.GetArrayLength().Should().Be(0);
    }

    private string BuildUrl() =>
        scenario.Location is null
            ? scenario.Url
            : $"{scenario.Url}?location={Uri.EscapeDataString(scenario.Location)}";
}
