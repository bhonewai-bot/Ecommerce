using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Data.Models;

[Index("delete_flag", Name = "ix_categories_delete_flag")]
public partial class category
{
    [Key]
    public int id { get; set; }

    [StringLength(150)]
    public string name { get; set; } = null!;

    public string? description { get; set; }

    public bool delete_flag { get; set; }

    [InverseProperty("category")]
    public virtual ICollection<product> products { get; set; } = new List<product>();
}
