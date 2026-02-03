namespace Ecommerce.WebApi.Middlewares;

public sealed class IdempotencyKeyMiddleware
{
    private const string HeaderName = "Idempotency-Key";
    private readonly RequestDelegate _next;

    public IdempotencyKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(HeaderName, out var value))
        {
            context.Items[HeaderName] = value.ToString();
        }

        await _next(context);
    }
}
