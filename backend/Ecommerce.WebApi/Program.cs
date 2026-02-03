using System.Diagnostics;
using Ecommerce.Application.Features.Categories.Admin;
using Ecommerce.Application.Features.Categories.Public;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Idempotency;
using Ecommerce.Application.Features.Orders.Admin;
using Ecommerce.Application.Features.Orders.Public;
using Ecommerce.Application.Features.Payments.Public;
using Ecommerce.Application.Features.Products.Admin;
using Ecommerce.Application.Features.Products.Public;
using Ecommerce.Infrastructure;
using System.Text.Json.Serialization;
using Ecommerce.WebApi.Errors;
using Ecommerce.WebApi.Middlewares;
using Serilog;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    /*.WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Hour,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760, // 10MB
        retainedFileCountLimit: 24)*/
    
    .CreateLogger();

var stripeSecretKey = builder.Configuration["Stripe:SecretKey"];
if (string.IsNullOrWhiteSpace(stripeSecretKey) ||
    stripeSecretKey.Contains("REPLACE_ME", StringComparison.OrdinalIgnoreCase))
{
    using var loggerFactory = LoggerFactory.Create(logging => logging.AddSerilog(Log.Logger));
    var logger = loggerFactory.CreateLogger("Startup");
    logger.LogError("Stripe:SecretKey is missing or placeholder.");
    throw new InvalidOperationException("Stripe:SecretKey is missing or placeholder.");
}

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var error = ApiErrorFactory.ValidationFailed(context.ModelState);
            return new BadRequestObjectResult(error);
        };
    });
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IAdminProductsService, AdminProductsService>();
builder.Services.AddScoped<IAdminCategoriesService, AdminCategoriesService>();
builder.Services.AddScoped<IAdminOrdersService, AdminOrdersService>();
builder.Services.AddScoped<IPublicProductsService, PublicProductsService>();
builder.Services.AddScoped<IPublicCategoriesService, PublicCategoriesService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IPublicOrdersService, PublicOrdersService>();
builder.Services.AddScoped<IPaymentsService, PaymentsService>();
builder.Services.AddScoped<IdempotencyService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendDev", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalException");
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        if (exception is not null)
        {
            logger.LogError(exception, "Unhandled exception");
        }

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ApiErrorFactory.Unexpected());
    });
});

app.UseCors("FrontendDev");

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<IdempotencyKeyMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnostic, context) =>
    {
        var correlationId = context.Items.TryGetValue("CorrelationId", out var value) ? value?.ToString() : null;
        diagnostic.Set("CorrelationId", correlationId);
        diagnostic.Set("TraceId", Activity.Current?.TraceId.ToString());
        diagnostic.Set("UserAgent", context.Request.Headers.UserAgent.ToString());
    };
});

app.UseAuthorization();

app.MapControllers();

app.Run();
