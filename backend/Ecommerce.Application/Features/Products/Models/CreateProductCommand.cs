namespace Ecommerce.Application.Features.Products.Models;

public sealed class CreateProductCommand
{
    public int CategoryId { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public decimal Price { get; init; }
    public string? ImageUrl { get; init; }
}
