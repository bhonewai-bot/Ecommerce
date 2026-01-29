using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Products.Models;

namespace Ecommerce.Application.Features.Products.Admin;

public sealed class AdminProductsService : IAdminProductsService
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;

    public AdminProductsService(IProductRepository products, ICategoryRepository categories)
    {
        _products = products;
        _categories = categories;
    }

    public async Task<AdminProductListResponse> GetAllAsync(AdminProductListQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await _products.GetProductsAsync(new ProductListQuery
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = query.CategoryId,
            Search = query.Search,
            SortBy = query.SortBy,
            SortOrder = query.SortOrder
        }, cancellationToken);

        return new AdminProductListResponse
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

    public async Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var categoryExists = await _categories.GetCategoryByIdAsync(request.CategoryId, cancellationToken);
        if (!categoryExists.IsSuccess)
        {
            return Result<ProductDto>.BadRequest("Category does not exist.");
        }

        return await _products.CreateAsync(new CreateProductCommand
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl
        }, cancellationToken);
    }

    public async Task<Result> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var categoryExists = await _categories.GetCategoryByIdAsync(request.CategoryId, cancellationToken);
        if (!categoryExists.IsSuccess)
        {
            return Result.BadRequest("Category does not exist.");
        }

        return await _products.UpdateAsync(id, new UpdateProductCommand
        {
            CategoryId = request.CategoryId,
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            ImageUrl = request.ImageUrl
        }, cancellationToken);
    }

    public Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return _products.DeleteAsync(id, cancellationToken);
    }
}
