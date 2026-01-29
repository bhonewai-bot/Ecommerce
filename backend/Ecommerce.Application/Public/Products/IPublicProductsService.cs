using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Public.Products;

public interface IPublicProductsService
{
    Task<PublicProductsListResult> GetAllAsync(
        int page,
        int pageSize,
        int? categoryId,
        string sortBy,
        string sortOrder,
        CancellationToken cancellationToken);

    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
}
