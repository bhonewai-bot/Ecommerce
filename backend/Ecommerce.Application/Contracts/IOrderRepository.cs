using Ecommerce.Application.Common;
using Ecommerce.Application.Orders;

namespace Ecommerce.Application.Contracts;

public interface IOrderRepository
{
    Task<Result> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);
}
