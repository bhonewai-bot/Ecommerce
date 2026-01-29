using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Contracts;

public interface ICategoryRepository
{
    Task<(IReadOnlyList<CategoryDto> Items, int TotalCount)> GetCategoriesAsync(
        int page,
        int pageSize,
        string? query,
        CancellationToken cancellationToken);

    Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
}
