using Ecommerce.Application.Features.Checkout;

namespace Ecommerce.Application.Features.Orders.Admin;

public sealed class AdminOrderListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public OrderStatus? Status { get; init; }
}
