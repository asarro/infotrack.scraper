using Infotrack.Scraper.Persistence;
using Npgsql;

namespace Infotrack.Scraper.Extensions;

internal static class DatabaseExtensions
{
    internal static IServiceCollection AddDatabase(
        this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException(
                                   "Connection string 'DefaultConnection' is not configured.");

        services.AddSingleton(NpgsqlDataSource.Create(connectionString));
        services.AddScoped<ISolicitorRepository, SolicitorRepository>();

        return services;
    }

}
