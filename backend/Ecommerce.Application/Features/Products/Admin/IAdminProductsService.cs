using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Products.Models;

namespace Ecommerce.Application.Features.Products.Admin;

public interface IAdminProductsService
{
    Task<AdminProductListResponse> GetAllAsync(AdminProductListQuery query, CancellationToken cancellationToken);
    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<ProductDto>> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, UpdateProductRequest request, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
