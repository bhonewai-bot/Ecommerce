namespace Ecommerce.Application.Features.Categories.Models;

public sealed class CategoryListQuery
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public string? Search { get; init; }
}
