using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Contracts;

namespace Ecommerce.Application.Public.Categories;

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
