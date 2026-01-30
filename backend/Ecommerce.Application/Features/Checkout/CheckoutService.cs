using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout.Commands;
using Ecommerce.Application.Features.Checkout.Models;
using Ecommerce.Application.Features.Products.Models;

namespace Ecommerce.Application.Features.Checkout;

public sealed class CheckoutService : ICheckoutService
{
    private const string DefaultCurrency = "THB";
    private readonly IProductRepository _products;
    private readonly IOrderRepository _orders;

    public CheckoutService(IProductRepository products, IOrderRepository orders)
    {
        _products = products;
        _orders = orders;
    }

    public async Task<Result<CheckoutResponse>> CreateOrderAsync(
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return Result<CheckoutResponse>.BadRequest("Order must contain at least one item.");
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

        return Result<CheckoutResponse>.Ok(new CheckoutResponse
        {
            PublicId = command.PublicId,
            TotalAmount = command.TotalAmount,
            Currency = command.Currency,
            Status = command.Status,
            Items = responseItems
        });
    }
}
