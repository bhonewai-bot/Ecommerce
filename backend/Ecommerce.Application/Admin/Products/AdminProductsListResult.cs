using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Admin.Products;

public sealed class AdminProductsListResult : PagedResult<ProductDto>
{
    public int? CategoryId { get; init; }
    public required string SortBy { get; init; }
    public required string SortOrder { get; init; }
}
