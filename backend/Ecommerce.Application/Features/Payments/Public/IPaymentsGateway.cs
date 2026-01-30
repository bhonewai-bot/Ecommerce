using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;

namespace Ecommerce.Application.Features.Payments.Public;

public interface IPaymentsGateway
{
    Task<Result<string>> CreatePaymentIntentAsync(
        long amount,
        string currency,
        Guid orderPublicId,
        CancellationToken cancellationToken);

    Task<Result<StripeWebhookEventDto>> ParseWebhookEventAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken);
}
