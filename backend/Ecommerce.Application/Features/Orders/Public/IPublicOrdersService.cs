using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Orders.Models;

namespace Ecommerce.Application.Features.Orders.Public;

public interface IPublicOrdersService
{
    Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
