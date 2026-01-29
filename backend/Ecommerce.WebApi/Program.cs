using Ecommerce.Application.Features.Categories.Admin;
using Ecommerce.Application.Features.Categories.Public;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Products.Admin;
using Ecommerce.Application.Features.Products.Public;
using Ecommerce.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<IAdminProductsService, AdminProductsService>();
builder.Services.AddScoped<IAdminCategoriesService, AdminCategoriesService>();
builder.Services.AddScoped<IPublicProductsService, PublicProductsService>();
builder.Services.AddScoped<IPublicCategoriesService, PublicCategoriesService>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
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

app.UseAuthorization();

app.MapControllers();

app.Run();
