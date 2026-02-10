using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Orders.Models;
using Ecommerce.Application.Features.Payments.Public;

namespace Ecommerce.Application.Features.Orders.Public;

public sealed class PublicOrdersService : IPublicOrdersService
{
    private readonly IOrderRepository _orders;
    private readonly IPaymentsGateway _payments;

    public PublicOrdersService(IOrderRepository orders, IPaymentsGateway payments)
    {
        _orders = orders;
        _payments = payments;
    }

    public Task<Result<OrderDto>> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        return _orders.GetByPublicIdAsync(publicId, cancellationToken);
    }

    public async Task<Result<OrderDto>> GetByCheckoutSessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var sessionResult = await _payments.GetCheckoutSessionAsync(sessionId, cancellationToken);
        if (!sessionResult.IsSuccess || sessionResult.Data is null)
        {
            return Result<OrderDto>.BadRequest(sessionResult.Error ?? "Unable to load checkout session.");
        }

        if (!sessionResult.Data.OrderPublicId.HasValue)
        {
            return Result<OrderDto>.BadRequest("Checkout session is missing order reference.");
        }

        return await _orders.GetByPublicIdAsync(sessionResult.Data.OrderPublicId.Value, cancellationToken);
    }

    public async Task<Result<CheckoutResumeDto>> GetResumableCheckoutBySessionIdAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var sessionResult = await _payments.GetCheckoutSessionAsync(sessionId, cancellationToken);
        if (!sessionResult.IsSuccess || sessionResult.Data is null)
        {
            return Result<CheckoutResumeDto>.BadRequest(
                sessionResult.Error ?? "Unable to load checkout session.");
        }

        if (!sessionResult.Data.OrderPublicId.HasValue)
        {
            return Result<CheckoutResumeDto>.BadRequest(
                "Checkout session is missing order reference.");
        }

        var orderResult = await _orders.GetByPublicIdAsync(
            sessionResult.Data.OrderPublicId.Value,
            cancellationToken);

        if (!orderResult.IsSuccess || orderResult.Data is null)
        {
            return orderResult.Status switch
            {
                ResultStatus.NotFound => Result<CheckoutResumeDto>.NotFound(),
                ResultStatus.Conflict => Result<CheckoutResumeDto>.Conflict(
                    orderResult.Error ?? "Order is not available."),
                _ => Result<CheckoutResumeDto>.BadRequest(
                    orderResult.Error ?? "Unable to load order.")
            };
        }

        if (orderResult.Data.Status != OrderStatus.PendingPayment)
        {
            return Result<CheckoutResumeDto>.Conflict("Order status is not pending.");
        }

        if (string.IsNullOrWhiteSpace(sessionResult.Data.Url))
        {
            return Result<CheckoutResumeDto>.Conflict(
                "Checkout session can no longer be resumed.");
        }

        return Result<CheckoutResumeDto>.Ok(
            new CheckoutResumeDto(
                sessionResult.Data.OrderPublicId.Value,
                sessionResult.Data.Url));
    }

    public async Task<Result<OrderDto>> CancelPendingAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var statusResult = await _orders.GetStatusByPublicIdAsync(publicId, cancellationToken);
        if (!statusResult.IsSuccess)
        {
            return statusResult.Status switch
            {
                ResultStatus.NotFound => Result<OrderDto>.NotFound(),
                _ => Result<OrderDto>.BadRequest(statusResult.Error ?? "Unable to load order.")
            };
        }

        if (statusResult.Data != OrderStatus.PendingPayment)
        {
            return Result<OrderDto>.Conflict("Order status is not pending.");
        }

        var updateResult = await _orders.UpdateStatusByPublicIdAsync(publicId, OrderStatus.Cancelled, cancellationToken);
        if (!updateResult.IsSuccess)
        {
            return updateResult.Status switch
            {
                ResultStatus.NotFound => Result<OrderDto>.NotFound(),
                ResultStatus.Conflict => Result<OrderDto>.Conflict(updateResult.Error ?? "Order status is not cancellable."),
                _ => Result<OrderDto>.BadRequest(updateResult.Error ?? "Unable to cancel order.")
            };
        }

        return await _orders.GetByPublicIdAsync(publicId, cancellationToken);
    }
}
