using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Products;

namespace Ecommerce.Application.Admin.Products;

public sealed class AdminProductsService : IAdminProductsService
{
    private readonly IProductRepository _products;

    public AdminProductsService(IProductRepository products)
    {
        _products = products;
    }

    public async Task<AdminProductsListResult> GetAllAsync(
        GetAdminProductsParams parameters,
        CancellationToken cancellationToken)
    {
        var page = parameters.Page < 1 ? 1 : parameters.Page;
        var pageSize = parameters.PageSize < 1 ? 10 : parameters.PageSize;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await _products.GetProductsAsync(new ProductListQuery
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = parameters.CategoryId,
            Search = parameters.Query,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder
        }, cancellationToken);

        return new AdminProductsListResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            CategoryId = parameters.CategoryId,
            SortBy = parameters.SortBy,
            SortOrder = parameters.SortOrder
        };
    }

    public async Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _products.GetProductByIdAsync(id, cancellationToken);
    }

    public async Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken)
    {
        return await _products.CreateAsync(dto, cancellationToken);
    }

    public async Task<Result> UpdateAsync(
        int id,
        ProductUpdateDto dto,
        CancellationToken cancellationToken)
    {
        return await _products.UpdateAsync(id, dto, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return await _products.DeleteAsync(id, cancellationToken);
    }
}
