namespace Ecommerce.Application.Admin.Categories;

public sealed class GetAdminCategoriesParams
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Query { get; init; }
}
