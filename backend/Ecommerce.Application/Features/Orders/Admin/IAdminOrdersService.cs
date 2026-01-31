using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Orders.Models;

namespace Ecommerce.Application.Features.Orders.Admin;

public interface IAdminOrdersService
{
    Task<AdminOrderListResponse> GetAllAsync(AdminOrderListQuery query, CancellationToken cancellationToken);
    Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result> UpdateStatusAsync(Guid publicId, OrderStatus status, CancellationToken cancellationToken);
}
