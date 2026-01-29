using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Public.Products;

public sealed class PublicProductsListResult : PagedResult<ProductDto>
{
    public int? CategoryId { get; init; }
    public required string SortBy { get; init; }
    public required string SortOrder { get; init; }
}
