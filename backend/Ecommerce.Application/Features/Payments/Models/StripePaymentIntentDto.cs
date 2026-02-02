namespace Ecommerce.Application.Features.Payments.Models;

public sealed record StripePaymentIntentDto(string PaymentIntentId, string ClientSecret);
