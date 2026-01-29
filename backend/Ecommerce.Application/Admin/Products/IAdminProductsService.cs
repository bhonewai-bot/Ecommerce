using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;

namespace Ecommerce.Application.Admin.Products;

public interface IAdminProductsService
{
    Task<AdminProductsListResult> GetAllAsync(GetAdminProductsParams parameters, CancellationToken cancellationToken);
    Task<Result<ProductDto>> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
