using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Orders.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Features.Orders.Admin;

public sealed class AdminOrdersService : IAdminOrdersService
{
    private readonly IOrderRepository _orders;
    private readonly ILogger<AdminOrdersService> _logger;

    public AdminOrdersService(IOrderRepository orders, ILogger<AdminOrdersService> logger)
    {
        _orders = orders;
        _logger = logger;
    }

    public Task<AdminOrderListResponse> GetAllAsync(AdminOrderListQuery query, CancellationToken cancellationToken)
    {
        return _orders.GetAdminListAsync(query, cancellationToken);
    }

    public Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return _orders.GetByPublicIdAsync(publicId, cancellationToken);
    }

    public async Task<Result> UpdateStatusAsync(Guid publicId, OrderStatus status, CancellationToken cancellationToken)
    {
        var currentStatusResult = await _orders.GetStatusByPublicIdAsync(publicId, cancellationToken);
        if (!currentStatusResult.IsSuccess)
        {
            return currentStatusResult.Status switch
            {
                ResultStatus.NotFound => Result.NotFound(),
                _ => Result.BadRequest(currentStatusResult.Error ?? "Unable to load order status.")
            };
        }

        var currentStatus = currentStatusResult.Data;
        if (currentStatus == status)
        {
            return Result.Ok();
        }

        if (!IsTransitionAllowed(currentStatus, status))
        {
            return Result.Conflict("Invalid order status transition.");
        }

        var updateResult = await _orders.UpdateStatusByPublicIdAsync(publicId, status, cancellationToken);
        if (!updateResult.IsSuccess)
        {
            return updateResult;
        }

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["OrderPublicId"] = publicId,
                   ["OldStatus"] = currentStatus,
                   ["NewStatus"] = status,
                   ["Source"] = "admin_api"
               }))
        {
            _logger.LogInformation("admin.order.status_changed {@Audit}",
                new
                {
                    OrderPublicId = publicId,
                    OldStatus = currentStatus,
                    NewStatus = status,
                    Source = "admin_api"
                });
        }

        return Result.Ok();
    }

    private static bool IsTransitionAllowed(OrderStatus current, OrderStatus next)
    {
        return current switch
        {
            OrderStatus.PendingPayment => next == OrderStatus.Cancelled,
            OrderStatus.Paid => next == OrderStatus.Fulfilled || next == OrderStatus.Cancelled,
            OrderStatus.Fulfilled => false,
            OrderStatus.Cancelled => false,
            _ => false
        };
    }
}
