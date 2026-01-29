using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Products;

namespace Ecommerce.Application.Contracts;

public interface IProductRepository
{
    Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> GetProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductDto>> GetProductsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken);

    Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken);
    Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken);
    Task<Result> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken);
    Task<Result> DeleteAsync(int id, CancellationToken cancellationToken);
}
