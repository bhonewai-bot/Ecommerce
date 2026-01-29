using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Categories.Models;

namespace Ecommerce.Application.Features.Categories.Public;

public sealed class PublicCategoriesService : IPublicCategoriesService
{
    private readonly ICategoryRepository _categories;

    public PublicCategoriesService(ICategoryRepository categories)
    {
        _categories = categories;
    }

    public Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return _categories.GetAllAsync(cancellationToken);
    }

    public Task<Result<CategoryDto>> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        return _categories.GetCategoryByIdAsync(id, cancellationToken);
    }
}
