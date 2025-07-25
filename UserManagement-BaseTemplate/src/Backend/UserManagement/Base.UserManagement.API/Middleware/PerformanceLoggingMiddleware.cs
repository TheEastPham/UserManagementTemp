using System.Diagnostics;

namespace Base.UserManagement.API.Middleware;

public class PerformanceLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PerformanceLoggingMiddleware> _logger;
    private static readonly ActivitySource ActivitySource = new("Base.UserManagement.API");

    public PerformanceLoggingMiddleware(RequestDelegate next, ILogger<PerformanceLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip logging for health checks and metrics endpoints
        if (context.Request.Path.StartsWithSegments("/health") ||
            context.Request.Path.StartsWithSegments("/metrics"))
        {
            await _next(context);
            return;
        }

        using var activity = ActivitySource.StartActivity($"{context.Request.Method} {context.Request.Path}");
        
        var stopwatch = Stopwatch.StartNew();
        var startTime = DateTimeOffset.UtcNow;

        try
        {
            // Add custom tags to the activity
            activity?.SetTag("http.method", context.Request.Method);
            activity?.SetTag("http.url", context.Request.Path);
            activity?.SetTag("http.scheme", context.Request.Scheme);
            activity?.SetTag("user.id", context.User?.Identity?.Name ?? "anonymous");

            await _next(context);

            stopwatch.Stop();

            // Add response information to activity
            activity?.SetTag("http.status_code", context.Response.StatusCode);
            activity?.SetTag("http.response_size", context.Response.ContentLength ?? 0);

            // Log performance metrics
            _logger.LogInformation(
                "Request {Method} {Path} completed in {ElapsedMs}ms with status {StatusCode}",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);

            // Log slow requests (> 1 second)
            if (stopwatch.ElapsedMilliseconds > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected: {Method} {Path} took {ElapsedMs}ms",
                    context.Request.Method,
                    context.Request.Path,
                    stopwatch.ElapsedMilliseconds);
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Record exception in activity
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.SetTag("error", true);
            activity?.SetTag("exception.type", ex.GetType().FullName);
            activity?.SetTag("exception.message", ex.Message);

            _logger.LogError(ex,
                "Request {Method} {Path} failed after {ElapsedMs}ms",
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
