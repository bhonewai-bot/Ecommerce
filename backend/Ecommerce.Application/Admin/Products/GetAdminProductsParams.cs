namespace Ecommerce.Application.Admin.Products;

public sealed class GetAdminProductsParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public int? CategoryId { get; init; }
    public string? Query { get; init; }
    public string SortBy { get; init; } = "id";
    public string SortOrder { get; init; } = "asc";
}
