using Ecommerce.Application.Features.Checkout;

namespace Ecommerce.Application.Features.Orders.Models;

public sealed record UpdateOrderStatusRequest(OrderStatus Status);
