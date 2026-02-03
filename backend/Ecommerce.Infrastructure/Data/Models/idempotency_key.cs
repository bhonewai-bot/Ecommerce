using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Infrastructure.Data.Models;

public partial class idempotency_key
{
    public int id { get; set; }

    [Column("idempotency_key")]
    public string idempotency_key_value { get; set; } = null!;

    public string scope { get; set; } = null!;

    public string request_hash { get; set; } = null!;

    public string status { get; set; } = null!;

    public int? response_code { get; set; }

    public string? response_body { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? completed_at { get; set; }
}
