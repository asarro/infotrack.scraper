using Infotrack.Scraper.Endpoints;
using Infotrack.Scraper.Extensions;
using Infotrack.Scraper.Middleware;
using Scalar.AspNetCore;
using Serilog;

// Bootstrap logger — captures failures before the host is built.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Debug("Starting Infotrack.Scraper");

    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console());

    builder.Services.AddLoggingServices(builder.Configuration);
    builder.Services.AddTelemetry(builder.Configuration);
    builder.Services.AddOpenApi();
    builder.Services.AddDatabase(builder.Configuration);
    builder.Services.AddCorsPolicy(builder.Configuration);
    builder.Services.AddServices(builder.Configuration);

    var app = builder.Build();

    app.UseMiddleware<LoggingMiddleware>();
    app.UseRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.MapScalarApiReference();
    }

    app.UseCors();
    app.MapHealthEndpoints();
    app.MapConveyancingEndpoints();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Infotrack.Scraper terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}

// Exposed so the test project's WebApplicationFactory can bootstrap the app.
public partial class Program;
