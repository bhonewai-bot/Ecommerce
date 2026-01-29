using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.Features.Products.Models;

public sealed class UpdateProductRequest
{
    [Required]
    public int CategoryId { get; set; }

    [Required, StringLength(200)]
    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    [Range(0, 99999999)]
    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }
}
