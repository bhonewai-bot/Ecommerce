namespace Ecommerce.Application.Features.Payments.Models;

public sealed record StripeWebhookEventDto(
    string StripeEventId,
    string Type,
    string? OrderPublicId,
    string? PaymentIntentId);
