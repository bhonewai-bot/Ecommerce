namespace Ecommerce.Application.Features.Categories.Models;

public sealed class CreateCategoryCommand
{
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
}
