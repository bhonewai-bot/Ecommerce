using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Products.Models;

namespace Ecommerce.Application.Features.Products.Admin;

public sealed class AdminProductListResponse : PagedResult<ProductDto>
{
    public int? CategoryId { get; init; }
    public required string SortBy { get; init; }
    public required string SortOrder { get; init; }
}
