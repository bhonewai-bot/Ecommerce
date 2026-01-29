namespace Ecommerce.Application.Features.Categories.Admin;

public sealed class AdminCategoryListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? Search { get; init; }
}
