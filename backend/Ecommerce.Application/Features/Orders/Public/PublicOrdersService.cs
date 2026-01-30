using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Orders.Models;

namespace Ecommerce.Application.Features.Orders.Public;

public sealed class PublicOrdersService : IPublicOrdersService
{
    private readonly IOrderRepository _orders;

    public PublicOrdersService(IOrderRepository orders)
    {
        _orders = orders;
    }

    public Task<Result<GetOrderResponse>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return _orders.GetByPublicIdAsync(publicId, cancellationToken);
    }
}
