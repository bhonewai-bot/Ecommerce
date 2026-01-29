namespace Ecommerce.Application.Orders;

public sealed class CheckoutRequestDto
{
    public string? CustomerEmail { get; set; }
    public List<CheckoutItemDto> Items { get; set; } = new();
}

public sealed class CheckoutItemDto
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}

public sealed class CheckoutResponseDto
{
    public Guid PublicId { get; init; }
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = null!;
    public OrderStatus Status { get; init; }
    public List<OrderItemDto> Items { get; init; } = new();
}

public sealed class OrderItemDto
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = null!;
    public decimal UnitPrice { get; init; }
    public int Quantity { get; init; }
    public decimal LineTotal { get; init; }
}
