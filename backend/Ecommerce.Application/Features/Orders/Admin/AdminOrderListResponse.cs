using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Orders.Models;

namespace Ecommerce.Application.Features.Orders.Admin;

public sealed class AdminOrderListResponse : PagedResult<OrderListItemDto>
{
    public OrderStatus? Status { get; init; }
}
