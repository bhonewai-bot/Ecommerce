using System.Diagnostics;
using Ecommerce.Application.Features.Categories.Admin;
using Ecommerce.Application.Features.Categories.Public;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Orders.Admin;
using Ecommerce.Application.Features.Orders.Public;
using Ecommerce.Application.Features.Payments.Public;
using Ecommerce.Application.Features.Products.Admin;
using Ecommerce.Application.Features.Products.Public;
using Ecommerce.Infrastructure;
using System.Text.Json.Serialization;
using Ecommerce.WebApi.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File("logs/log-.txt",
        rollingInterval: RollingInterval.Hour,
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [{CorrelationId}] {Message:lj}{NewLine}{Exception}",
        fileSizeLimitBytes: 10485760, // 10MB
        retainedFileCountLimit: 24)
    
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
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

app.UseCors("FrontendDev");

app.UseMiddleware<CorrelationIdMiddleware>();

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
