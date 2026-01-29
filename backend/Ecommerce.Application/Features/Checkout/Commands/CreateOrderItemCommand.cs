namespace Ecommerce.Application.Features.Checkout.Commands;

public sealed class CreateOrderItemCommand
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
