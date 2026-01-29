using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Categories.Models;

namespace Ecommerce.Application.Contracts;

public interface ICategoryRepository
{
    Task<(IReadOnlyList<CategoryDto> Items, int TotalCount)> GetCategoriesAsync(
        CategoryListQuery query,
        CancellationToken cancellationToken);

    Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, UpdateCategoryCommand command, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);

    Task<bool> HasActiveProductsAsync(int id, CancellationToken cancellationToken);
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
}
