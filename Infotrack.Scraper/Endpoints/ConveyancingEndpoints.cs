using Infotrack.Scraper.Conveyancing;
using Infotrack.Scraper.Models;

namespace Infotrack.Scraper.Endpoints;

internal static class ConveyancingEndpoints
{
    internal static WebApplication MapConveyancingEndpoints(this WebApplication app)
    {
        app.MapGet("/conveyancing/locations",
            (ILocationProvider provider) => TypedResults.Ok(provider.GetLocations()));

        app.MapGet("/conveyancing/solicitors", async (
            string location,
            ISolicitorSearchService service,
            HttpResponse response,
            CancellationToken ct) =>
        {
            var result = await service.SearchAsync(location.ToLowerInvariant(), ct);

            if (result.IsSuccess)
                return Results.Ok(result.Value);

            if (result.Error is WarmingUpError)
            {
                // Data is still being collected — ask the client to retry shortly.
                response.Headers.RetryAfter = "60";
                return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }

            return Results.BadRequest(new { result.Error.Message });
        });

        return app;
    }
}
