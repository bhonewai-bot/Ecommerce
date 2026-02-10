using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout.Commands;
using Ecommerce.Application.Features.Checkout.Models;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Payments.Public;
using Ecommerce.Application.Features.Products.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Application.Features.Checkout;

public sealed class CheckoutService : ICheckoutService
{
    private const string DefaultCurrency = "THB";
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;
    private readonly IPaymentsGateway _payments;
    private readonly ILogger<CheckoutService> _logger;
    private readonly string _frontendBaseUrl;

    public CheckoutService(
        IProductRepository products,
        IOrderRepository orders,
        IPaymentsGateway payments,
        IConfiguration configuration,
        ILogger<CheckoutService> logger)
    {
        _products = products;
        _orders = orders;
        _payments = payments;
        _logger = logger;
        _frontendBaseUrl = configuration["Client:BaseUrl"] ?? string.Empty;
    }

    public async Task<Result<CheckoutResponse>> CreateOrderAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return Result<CheckoutResponse>.BadRequest("Order must contain at least one item.");
        }

        if (string.IsNullOrWhiteSpace(_frontendBaseUrl))
        {
            return Result<CheckoutResponse>.BadRequest("Frontend base URL is not configured.");
        }

        if (request.Items.Any(item => item.Quantity <= 0))
        {
            return Result<CheckoutResponse>.BadRequest("Quantity must be greater than zero.");
        }

        var productIds = request.Items
            .Select(item => item.ProductId)
            .Distinct()
            .ToArray();

        var products = await _products.GetProductsByIdsAsync(productIds, cancellationToken);
        if (products.Count != productIds.Length)
        {
            return Result<CheckoutResponse>.NotFound();
        }

        var productLookup = products.ToDictionary(p => p.Id);
        var responseItems = new List<OrderItemDto>(request.Items.Count);
        var commandItems = new List<CreateOrderItemCommand>(request.Items.Count);
        decimal subtotal = 0m;

        foreach (var item in request.Items)
        {
            if (!productLookup.TryGetValue(item.ProductId, out var product))
            {
                return Result<CheckoutResponse>.NotFound();
            }

            var unitPrice = product.Price;
            var lineTotal = unitPrice * item.Quantity;
            subtotal += lineTotal;

            responseItems.Add(new OrderItemDto(
                product.Id,
                product.Name,
                unitPrice,
                item.Quantity,
                lineTotal));

            commandItems.Add(new CreateOrderItemCommand
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                LineTotal = lineTotal
            });
        }

        var command = new CreateOrderCommand
        {
            PublicId = Guid.NewGuid(),
            CustomerEmail = request.CustomerEmail,
            Currency = DefaultCurrency,
            Status = OrderStatus.PendingPayment,
            SubtotalAmount = subtotal,
            DiscountAmount = 0m,
            TaxAmount = 0m,
            TotalAmount = subtotal,
            Items = commandItems
        };

        var createResult = await _orders.CreateAsync(command, cancellationToken);
        if (!createResult.IsSuccess)
        {
            return new Result<CheckoutResponse>(createResult.Status, default, createResult.Error);
        }

        var lineItems = responseItems
            .Select(item => new StripeCheckoutLineItemDto(
                item.ProductName,
                ToMinorUnits(item.UnitPrice),
                item.Quantity))
            .ToList();

        var successUrl = BuildReturnUrl(command.PublicId, "success");
        var cancelUrl = BuildReturnUrl(command.PublicId, "cancel");

        var sessionResult = await _payments.CreateCheckoutSessionAsync(
            new StripeCheckoutSessionRequest(
                command.PublicId,
                command.Currency,
                request.CustomerEmail,
                lineItems,
                successUrl,
                cancelUrl),
            cancellationToken);

        if (!sessionResult.IsSuccess || sessionResult.Data is null)
        {
            await _orders.UpdateStatusByPublicIdAsync(command.PublicId, OrderStatus.Cancelled, cancellationToken);
            return Result<CheckoutResponse>.BadRequest(
                sessionResult.Error ?? "Unable to start checkout session.");
        }

        var attachSessionResult = await _orders.UpdateCheckoutSessionIdByPublicIdAsync(
            command.PublicId,
            sessionResult.Data.SessionId,
            cancellationToken);
        if (!attachSessionResult.IsSuccess)
        {
            return Result<CheckoutResponse>.BadRequest(
                attachSessionResult.Error ?? "Unable to store checkout session.");
        }

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["OrderPublicId"] = command.PublicId,
                   ["CheckoutSessionId"] = sessionResult.Data.SessionId,
                   ["Source"] = "public_api"
               }))
        {
            _logger.LogInformation(
                "checkout.created {@Audit}",
                new
                {
                    OrderPublicId = command.PublicId,
                    CheckoutSessionId = sessionResult.Data.SessionId,
                    TotalAmount = command.TotalAmount,
                    Currency = command.Currency,
                    Source = "public_api"
                });
        }

        return Result<CheckoutResponse>.Ok(new CheckoutResponse
        {
            OrderPublicId = command.PublicId,
            TotalAmount = command.TotalAmount,
            Currency = command.Currency,
            Status = command.Status,
            Items = responseItems,
            StripeCheckoutUrl = sessionResult.Data.Url,
            CheckoutSessionId = sessionResult.Data.SessionId
        });
    }

    private string BuildReturnUrl(Guid publicId, string outcome)
    {
        var baseUrl = _frontendBaseUrl.TrimEnd('/');
        return $"{baseUrl}/checkout/{outcome}?order={publicId}&session_id={{CHECKOUT_SESSION_ID}}";
    }

    private static long ToMinorUnits(decimal amount)
    {
        var scaled = decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        return decimal.ToInt64(scaled);
    }
}
