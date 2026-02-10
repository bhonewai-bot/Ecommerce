using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Orders.Models;

namespace Ecommerce.Application.Features.Orders.Public;

public interface IPublicOrdersService
{
    Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result<OrderDto>> GetByCheckoutSessionIdAsync(string sessionId, CancellationToken cancellationToken);
    Task<Result<CheckoutResumeDto>> GetResumableCheckoutBySessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken);
    Task<Result<OrderDto>> CancelPendingAsync(Guid publicId, CancellationToken cancellationToken);
}
