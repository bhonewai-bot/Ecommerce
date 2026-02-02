using Ecommerce.Application.Contracts;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Payments;

public sealed class StripeEventDeduper : IStripeEventDeduper
{
    private readonly EcommerceDbContext _db;

    public StripeEventDeduper(EcommerceDbContext db)
    {
        _db = db;
    }

    public async Task<bool> TryMarkProcessedAsync(
        string stripeEventId,
        string eventType,
        Guid? orderPublicId,
        string? paymentIntentId,
        CancellationToken cancellationToken)
    {
        var result = await _db.processed_stripe_events
            .FromSqlInterpolated($@"
                INSERT INTO processed_stripe_events
                    (stripe_event_id, event_type, order_public_id, payment_intent_id)
                VALUES
                    ({stripeEventId}, {eventType}, {orderPublicId}, {paymentIntentId})
                ON CONFLICT (stripe_event_id) DO NOTHING
                RETURNING id
            ")
            .AsNoTracking()
            .Select(e => e.id)
            .FirstOrDefaultAsync(cancellationToken);

        return result != 0;
    }
}
