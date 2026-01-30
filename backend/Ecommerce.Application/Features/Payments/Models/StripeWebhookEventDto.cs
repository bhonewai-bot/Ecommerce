namespace Ecommerce.Application.Features.Payments.Models;

public sealed record StripeWebhookEventDto(string Type, string? OrderPublicId);
