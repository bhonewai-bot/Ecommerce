namespace Ecommerce.Application.Features.Payments.Models;

public sealed record StripeCheckoutSessionRequest(
    Guid OrderPublicId,
    string Currency,
    string? CustomerEmail,
    IReadOnlyList<StripeCheckoutLineItemDto> LineItems,
    string SuccessUrl,
    string CancelUrl);
