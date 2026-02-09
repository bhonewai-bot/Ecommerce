using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;

namespace Ecommerce.Application.Features.Payments.Public;

public interface IPaymentsGateway
{
    Task<Result<StripePaymentIntentDto>> CreatePaymentIntentAsync(
        long amount,
        string currency,
        Guid orderPublicId,
        string? idempotencyKey,
        CancellationToken cancellationToken);

    Task<Result<StripeCheckoutSessionDto>> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken);

    Task<Result<StripeCheckoutSessionDto>> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken);

    Task<Result<StripeWebhookEventDto>> ParseWebhookEventAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken);
}
