using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Checkout.Models;

namespace Ecommerce.Application.Features.Orders.Models;

public sealed class GetOrderResponse
{
    public Guid PublicId { get; init; }
    public OrderStatus Status { get; init; }
    public string Currency { get; init; } = null!;
    public decimal SubtotalAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}
