using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Categories.Models;

namespace Ecommerce.Application.Features.Categories.Admin;

public sealed class AdminCategoryListResponse : PagedResult<CategoryDto>
{
    public string? Search { get; init; }
}
