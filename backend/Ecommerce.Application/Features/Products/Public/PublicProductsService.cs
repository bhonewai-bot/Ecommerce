using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Products.Models;

namespace Ecommerce.Application.Features.Products.Public;

public sealed class PublicProductsService : IPublicProductsService
{
    private readonly IProductRepository _products;

    public PublicProductsService(IProductRepository products)
    {
        _products = products;
    }

    public async Task<PublicProductListResponse> GetAllAsync(PublicProductListQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await _products.GetProductsAsync(new ProductListQuery
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = query.CategoryId,
            Search = null,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        }, cancellationToken);

        return new PublicProductListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            CategoryId = query.CategoryId,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        };
    }

    public Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _products.GetProductByIdAsync(id, cancellationToken);
    }
}
