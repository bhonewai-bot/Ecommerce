using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Public.Categories;

public interface IPublicCategoriesService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
}
