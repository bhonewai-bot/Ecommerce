using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;

namespace Ecommerce.Application.Features.Payments.Public;

public interface IPaymentsService
{
    Task<Result<PaymentIntentResponse>> CreatePaymentIntentAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result> HandleStripeWebhookAsync(string payload, string signatureHeader, CancellationToken cancellationToken);
}
