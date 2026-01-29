using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Categories.Models;

namespace Ecommerce.Application.Features.Categories.Admin;

public sealed class AdminCategoriesService : IAdminCategoriesService
{
    private readonly ICategoryRepository _categories;

    public AdminCategoriesService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<AdminCategoryListResponse> GetAllAsync(AdminCategoryListQuery query, CancellationToken cancellationToken)
    {
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize < 1 ? 10 : query.PageSize;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await _categories.GetCategoriesAsync(new CategoryListQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = query.Search
        }, cancellationToken);

        return new AdminCategoryListResponse
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Search = query.Search
        };
    }

    public Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _categories.GetCategoryByIdAsync(id, cancellationToken);
    }

    public Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        return _categories.CreateAsync(new CreateCategoryCommand
        {
            Name = request.Name,
            Description = request.Description
        }, cancellationToken);
    }

    public Task<Result> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        return _categories.UpdateAsync(id, new UpdateCategoryCommand
        {
            Name = request.Name,
            Description = request.Description
        }, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var hasActiveProducts = await _categories.HasActiveProductsAsync(id, cancellationToken);
        if (hasActiveProducts)
        {
            return Result.Conflict("Category has active products. Delete products first.");
        }

        return await _categories.DeleteAsync(id, cancellationToken);
    }
}
