using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Categories.Models;

namespace Ecommerce.Application.Features.Categories.Public;

public interface IPublicCategoriesService
{
    Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken);
    Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
}
