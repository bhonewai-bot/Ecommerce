namespace Ecommerce.Application.Features.Payments.Models;

public sealed record StripeCheckoutLineItemDto(
    string Name,
    long UnitAmount,
    long Quantity);
