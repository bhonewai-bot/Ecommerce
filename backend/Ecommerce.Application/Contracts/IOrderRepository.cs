using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout.Commands;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Orders.Models;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Orders.Admin;

namespace Ecommerce.Application.Contracts;

public interface IOrderRepository
{
    Task<Result> CreateAsync(CreateOrderCommand command, CancellationToken cancellationToken);
    Task<Result> UpdateCheckoutSessionIdByPublicIdAsync(Guid publicId, string checkoutSessionId, CancellationToken cancellationToken);
    Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result<OrderPaymentInfoDto>> GetPaymentInfoByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result> MarkPaidByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<AdminOrderListResponse> GetAdminListAsync(AdminOrderListQuery query, CancellationToken cancellationToken);
    Task<Result<OrderStatus>> GetStatusByPublicIdAsync(Guid publicId, CancellationToken cancellationToken);
    Task<Result> UpdateStatusByPublicIdAsync(Guid publicId, OrderStatus status, CancellationToken cancellationToken);
}
