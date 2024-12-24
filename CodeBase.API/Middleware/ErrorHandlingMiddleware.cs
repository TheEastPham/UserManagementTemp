using System.Net;
using CodeBase.API.Domain.Model;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;

namespace CodeBase.API.Middleware;

public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly TelemetryClient _telemetry;

    public ErrorHandlingMiddleware(RequestDelegate next, TelemetryClient telemetry)
    {
        this._next = next;
        this._telemetry = telemetry;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        object? errors = null;
        switch (ex)
        {
            case ApiException re:
                errors = new
                {
                    code = re.ErrorCode,
                    success = false,
                    message = re.Errors
                };
                context.Response.StatusCode = (int) HttpStatusCode.BadRequest;
                break;
            case { } e:
                _telemetry?.TrackException(ex);
                _telemetry?.TrackEvent("SERVER ERROR");
                errors = new
                {
                    code = e.HResult,
                    success = false,
                    message = e.Message
                };
                context.Response.StatusCode = (int) HttpStatusCode.InternalServerError;
                break;
        }

        context.Response.ContentType = "application/json";
        if (errors != null)
        {
            var result = JsonConvert.SerializeObject(errors);
            await context.Response.WriteAsync(result);
        }
    }
}