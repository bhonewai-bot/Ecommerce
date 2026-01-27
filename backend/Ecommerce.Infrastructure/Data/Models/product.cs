using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data.Models;

[Index("category_id", Name = "ix_products_category_id")]
[Index("delete_flag", Name = "ix_products_delete_flag")]
public partial class product
{
    [Key]
    public int id { get; set; }

    public int category_id { get; set; }

    [StringLength(200)]
    public string name { get; set; } = null!;

    public string? description { get; set; }

    [Precision(10, 2)]
    public decimal price { get; set; }

    public string? image_url { get; set; }

    public bool delete_flag { get; set; }

    [ForeignKey("category_id")]
    [InverseProperty("products")]
    public virtual category category { get; set; } = null!;
}
