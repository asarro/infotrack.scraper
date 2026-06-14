internal static class CorsExtensions
{
    internal static IServiceCollection AddCorsPolicy(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCors(options =>
            options.AddDefaultPolicy(policy =>
                policy.WithOrigins(configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                                   ?? ["http://localhost:5173"])
                    .AllowAnyMethod()
                    .AllowAnyHeader()));

        return services;
    }
}
