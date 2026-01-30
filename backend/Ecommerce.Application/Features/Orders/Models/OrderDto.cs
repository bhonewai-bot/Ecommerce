using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Checkout.Models;

namespace Ecommerce.Application.Features.Orders.Models;

public sealed record OrderDto(
    Guid PublicId,
    OrderStatus Status,
    string Currency,
    decimal SubtotalAmount,
    decimal DiscountAmount,
    decimal TaxAmount,
    decimal TotalAmount,
    List<OrderItemDto> Items);
