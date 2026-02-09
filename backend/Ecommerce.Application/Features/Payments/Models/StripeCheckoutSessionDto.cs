namespace Ecommerce.Application.Features.Payments.Models;

public sealed record StripeCheckoutSessionDto(
    string SessionId,
    string? Url,
    Guid? OrderPublicId);
