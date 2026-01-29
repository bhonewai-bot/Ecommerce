namespace Ecommerce.Application.Features.Products.Admin;

public sealed class AdminProductListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? CategoryId { get; init; }
    public string? Search { get; init; }
    public string SortBy { get; init; } = "id";
    public string SortOrder { get; init; } = "asc";
}
