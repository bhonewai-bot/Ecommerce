using System;
using System.Collections.Generic;

namespace Ecommerce.Infrastructure.Data.Models;

public partial class order_item
{
    public int id { get; set; }

    public int order_id { get; set; }

    public int? product_id { get; set; }

    public string product_name { get; set; } = null!;

    public decimal unit_price { get; set; }

    public int quantity { get; set; }

    public decimal line_total { get; set; }

    public virtual order order { get; set; } = null!;

    public virtual product? product { get; set; }
}
