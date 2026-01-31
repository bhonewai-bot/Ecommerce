using Ecommerce.Application.Features.Checkout;

namespace Ecommerce.Application.Features.Orders.Models;

public sealed record OrderListItemDto(
    Guid PublicId,
    OrderStatus Status,
    string Currency,
    decimal TotalAmount,
    string? CustomerEmail,
    DateTime CreatedAt);
