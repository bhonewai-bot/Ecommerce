using System;
using System.Collections.Generic;

namespace Ecommerce.Infrastructure.Data.Models;

public partial class product
{
    public int id { get; set; }

    public int category_id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public decimal price { get; set; }

    public string? image_url { get; set; }

    public bool delete_flag { get; set; }

    public virtual category category { get; set; } = null!;

    public virtual ICollection<order_item> order_items { get; set; } = new List<order_item>();
}
