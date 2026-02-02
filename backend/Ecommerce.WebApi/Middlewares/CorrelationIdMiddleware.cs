using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace Ecommerce.WebApi.Middlewares;

public sealed class CorrelationIdMiddleware
{
    private const string HeaderName = "X-Correlation-ID";
    private const string ItemKey = "CorrelationId";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var value)
            ? value.ToString()
            : Activity.Current?.TraceId.ToString()
            ?? Guid.NewGuid().ToString();
        
        context.Items[ItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        var traceId = Activity.Current?.TraceId.ToString();
        
        using (LogContext.PushProperty(ItemKey, correlationId))
        using (LogContext.PushProperty("TraceId", traceId))
        {
            await _next(context);
        }
        
    }
}
