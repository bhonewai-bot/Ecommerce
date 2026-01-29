using System;
using System.Collections.Generic;

namespace Ecommerce.Infrastructure.Data.Models;

public partial class category
{
    public int id { get; set; }

    public string name { get; set; } = null!;

    public string? description { get; set; }

    public bool delete_flag { get; set; }

    public virtual ICollection<product> products { get; set; } = new List<product>();
}
