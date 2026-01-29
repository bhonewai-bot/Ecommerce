using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Categories.Models;

namespace Ecommerce.Application.Features.Categories.Admin;

public interface IAdminCategoriesService
{
    Task<AdminCategoryListResponse> GetAllAsync(AdminCategoryListQuery query, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<CategoryDto>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
