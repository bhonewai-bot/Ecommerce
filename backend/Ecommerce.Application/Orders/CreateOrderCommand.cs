namespace Ecommerce.Application.Orders;

public sealed class CreateOrderCommand
{
    public Guid PublicId { get; init; }
    public string Currency { get; init; } = null!;
    public OrderStatus Status { get; init; }
    public decimal SubtotalAmount { get; init; }
    public decimal DiscountAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal TotalAmount { get; init; }
    public string? CustomerEmail { get; init; }
    public List<CreateOrderItemCommand> Items { get; init; } = new();
}

public sealed class CreateOrderItemCommand
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
