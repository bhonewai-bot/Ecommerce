using System;

namespace Ecommerce.Infrastructure.Data.Models;

public partial class processed_stripe_event
{
    public int id { get; set; }

    public string stripe_event_id { get; set; } = null!;

    public string event_type { get; set; } = null!;

    public Guid? order_public_id { get; set; }

    public string? payment_intent_id { get; set; }

    public DateTime created_at { get; set; }
}
