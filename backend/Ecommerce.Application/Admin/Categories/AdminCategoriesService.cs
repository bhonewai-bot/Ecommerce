using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Contracts;

namespace Ecommerce.Application.Admin.Categories;

public sealed class AdminCategoriesService : IAdminCategoriesService
{
    private readonly ICategoryRepository _categories;

    public AdminCategoriesService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public async Task<AdminCategoriesListResult> GetAllAsync(
        GetAdminCategoriesParams parameters,
        CancellationToken cancellationToken)
    {
        var page = parameters.Page < 1 ? 1 : parameters.Page;
        var pageSize = parameters.PageSize < 1 ? 10 : parameters.PageSize;
        if (pageSize > 100) pageSize = 100;

        var (items, totalCount) = await _categories.GetCategoriesAsync(
            page,
            pageSize,
            parameters.Query,
            cancellationToken);

        return new AdminCategoriesListResult
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            Query = parameters.Query
        };
    }

    public async Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _categories.GetCategoryByIdAsync(id, cancellationToken);
    }

    public async Task<Result<CategoryDto>> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken)
    {
        return await _categories.CreateAsync(dto, cancellationToken);
    }

    public async Task<Result> UpdateAsync(
        int id,
        CategoryUpdateDto dto,
        CancellationToken cancellationToken)
    {
        return await _categories.UpdateAsync(id, dto, cancellationToken);
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        return await _categories.DeleteAsync(id, cancellationToken);
    }
}
