using Infotrack.Scraper.Conveyancing;

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
            CancellationToken ct) =>
        {
            var result = await service.SearchAsync(location, ct);

            return result switch
            {
                { IsSuccess: true } => Results.Ok(),
                { IsFailure: true } => Results.BadRequest(new { result.Error.Message }),
                _ => Results.Problem()
            };
        });

        return app;
    }
}