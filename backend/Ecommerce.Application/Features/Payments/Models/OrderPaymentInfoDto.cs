using Ecommerce.Application.Features.Checkout;

namespace Ecommerce.Application.Features.Payments.Models;

public sealed record OrderPaymentInfoDto(
    Guid PublicId,
    OrderStatus Status,
    string Currency,
    decimal TotalAmount);
