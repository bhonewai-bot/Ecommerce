using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout.Commands;
using Ecommerce.Application.Features.Orders.Models;

namespace Ecommerce.Application.Contracts;

public interface IOrderRepository
{
    Task<Result> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);
    Task<Result<GetOrderResponse>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
}
