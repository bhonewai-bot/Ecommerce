using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Admin.Categories;

public interface IAdminCategoriesService
{
    Task<AdminCategoriesListResult> GetAllAsync(GetAdminCategoriesParams parameters, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
