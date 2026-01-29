using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Products.Models;

namespace Ecommerce.Application.Features.Products.Public;

public interface IPublicProductsService
{
    Task<PublicProductListResponse> GetAllAsync(PublicProductListQuery query, CancellationToken cancellationToken);
    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
}
