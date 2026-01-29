using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Admin.Categories;

public sealed class AdminCategoriesListResult : PagedResult<CategoryDto>
{
    public string? Query { get; init; }
}
