using Ecommerce.Application.Common;

namespace Ecommerce.Application.Contracts;

public interface IStripeEventDeduper
{
    Task<bool> TryMarkProcessedAsync(
        string stripeEventId,
        string eventType,
        Guid? orderPublicId,
        string? paymentIntentId,
        CancellationToken cancellationToken);
}
