namespace Ecommerce.Application.Features.Products.Models;

public sealed record ProductDto(
    int Id,
    int CategoryId,
    string Name,
    string? Description,
    decimal Price,
    string? ImageUrl);
