using System.Diagnostics;

namespace Infotrack.Scraper.Middleware;

internal sealed class LoggingMiddleware(RequestDelegate next)
{
    private const string CorrelationIdHeader = "X-Correlation-Id";

    public Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
                            ?? Guid.NewGuid().ToString();

        context.Response.Headers[CorrelationIdHeader] = correlationId;

        var activity = Activity.Current;

        ILogEventEnricher[] enrichers =
        [
            new PropertyEnricher("CorrelationId", correlationId),
            new PropertyEnricher("HttpMethod", context.Request.Method),
            new PropertyEnricher("RequestPath", context.Request.Path.Value),
            new PropertyEnricher("QueryString", context.Request.QueryString.Value ?? string.Empty),
            new PropertyEnricher("TraceId", activity?.TraceId.ToString() ?? string.Empty),
            new PropertyEnricher("SpanId", activity?.SpanId.ToString() ?? string.Empty),
        ];

        using (LogContext.Push(enrichers))
        {
            return next(context);
        }
    }
}
