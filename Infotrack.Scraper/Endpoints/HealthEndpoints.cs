using Npgsql;
using Serilog;

namespace Infotrack.Scraper.Endpoints;

internal static class HealthEndpoints
{
    internal static WebApplication MapHealthEndpoints(this WebApplication app)
    {
        app.MapGet("/health", async (NpgsqlDataSource dataSource, CancellationToken cancellationToken) =>
        {
            try
            {
                await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
                await using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1";
                await command.ExecuteScalarAsync(cancellationToken);
                return Results.Ok(new { status = "healthy" });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Health check failed: could not reach Postgres");
                return Results.Problem("Database is not reachable.", statusCode: StatusCodes.Status503ServiceUnavailable);
            }
        });

        return app;
    }
}