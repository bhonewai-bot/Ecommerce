using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Products;

namespace Ecommerce.Application.Public.Products;

public sealed class PublicProductsService : IPublicProductsService
{
    private readonly IProductRepository _products;

    public PublicProductsService(IProductRepository products)
    {
        _products = products;
    }

    public async Task<PublicProductsListResult> GetAllAsync(
        int page,
        int pageSize,
        int? categoryId,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken)
    {
        var normalizedPage = page < 1 ? 1 : page;
        var normalizedPageSize = pageSize < 1 ? 10 : pageSize;
        if (normalizedPageSize > 100) normalizedPageSize = 100;

        var (items, totalCount) = await _products.GetProductsAsync(new ProductListQuery
        {
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            CategoryId = categoryId,
            Search = null,
            SortBy = sortBy,
            SortOrder = sortOrder
        }, cancellationToken);

        return new PublicProductsListResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = normalizedPage,
            PageSize = normalizedPageSize,
            CategoryId = categoryId,
            SortBy = sortBy,
            SortOrder = sortOrder
        };
    }

    public Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _products.GetProductByIdAsync(id, cancellationToken);
    }
}
