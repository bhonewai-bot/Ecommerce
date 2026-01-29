using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Contracts;

namespace Ecommerce.Application.Orders;

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

    public async Task<Result<CheckoutResponseDto>> CreateOrderAsync(
        CheckoutRequestDto request,
        CancellationToken cancellationToken)
    {
        if (request.Items is null || request.Items.Count == 0)
        {
            return Result<CheckoutResponseDto>.BadRequest("Order must contain at least one item.");
        }

        if (request.Items.Any(item => item.Quantity <= 0))
        {
            return Result<CheckoutResponseDto>.BadRequest("Quantity must be greater than zero.");
        }

        var productIds = request.Items
            .Select(item => item.ProductId)
            .Distinct()
            .ToArray();

        var products = await _products.GetProductsByIdsAsync(productIds, cancellationToken);
        if (products.Count != productIds.Length)
        {
            return Result<CheckoutResponseDto>.NotFound();
        }

        var productLookup = products.ToDictionary(p => p.Id);
        var responseItems = new List<OrderItemDto>(request.Items.Count);
        var commandItems = new List<CreateOrderItemCommand>(request.Items.Count);
        decimal subtotal = 0m;

        foreach (var item in request.Items)
        {
            if (!productLookup.TryGetValue(item.ProductId, out var product))
            {
                return Result<CheckoutResponseDto>.NotFound();
            }

            var unitPrice = product.Price;
            var lineTotal = unitPrice * item.Quantity;
            subtotal += lineTotal;

            responseItems.Add(new OrderItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                UnitPrice = unitPrice,
                Quantity = item.Quantity,
                LineTotal = lineTotal
            });

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
            return new Result<CheckoutResponseDto>(createResult.Status, default, createResult.Error);
        }

        return Result<CheckoutResponseDto>.Ok(new CheckoutResponseDto
        {
            PublicId = command.PublicId,
            TotalAmount = command.TotalAmount,
            Currency = command.Currency,
            Status = command.Status,
            Items = responseItems
        });
    }
}
