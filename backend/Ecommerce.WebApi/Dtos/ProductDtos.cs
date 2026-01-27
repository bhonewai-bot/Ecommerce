using System.ComponentModel.DataAnnotations;

namespace Ecommerce.WebApi.Dtos;

public sealed record ProductDto(
    int Id,
    int CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl);

public sealed class ProductCreateDto
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

public sealed class ProductUpdateDto
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
