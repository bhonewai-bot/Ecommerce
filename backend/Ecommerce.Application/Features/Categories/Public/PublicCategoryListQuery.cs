namespace Ecommerce.Application.Features.Categories.Public;

public sealed class PublicCategoryListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
