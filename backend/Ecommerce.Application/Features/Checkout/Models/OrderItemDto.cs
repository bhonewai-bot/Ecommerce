namespace Ecommerce.Application.Features.Checkout.Models;

public sealed class OrderItemDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
