namespace Ecommerce.Application.Features.Checkout.Models;

public sealed class CheckoutResponse
{
    public Guid PublicId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = null!;
    public OrderStatus Status { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}
