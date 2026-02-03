using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Payments.Public;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Payments;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Stripe;

namespace Ecommerce.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DbConnection");

        services.AddDbContext<EcommerceDbContext>(options =>
            options.UseNpgsql(connectionString));

        var stripeSecretKey = configuration["Stripe:SecretKey"] ?? string.Empty;
        services.AddSingleton<IStripeClient>(new StripeClient(stripeSecretKey));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IPaymentsGateway, StripePaymentsGateway>();
        services.AddScoped<IStripeEventDeduper, StripeEventDeduper>();
        return services;
    }
}
