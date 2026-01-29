namespace Ecommerce.Application.Features.Products.Models;

public sealed class ProductListQuery
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int? CategoryId { get; init; }
    public string? Search { get; init; }
    public string SortBy { get; init; } = "id";
    public string SortOrder { get; init; } = "asc";
}
