using System;
using System.Collections.Generic;

namespace Ecommerce.Infrastructure.Data.Models;

public partial class order
{
    public int id { get; set; }

    public Guid public_id { get; set; }

    public short status { get; set; }

    public decimal subtotal_amount { get; set; }

    public decimal discount_amount { get; set; }

    public decimal tax_amount { get; set; }

    public decimal total_amount { get; set; }

    public string currency { get; set; } = null!;

    public string? customer_email { get; set; }

    public string? checkout_session_id { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }

    public virtual ICollection<order_item> order_items { get; set; } = new List<order_item>();
}
