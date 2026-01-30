using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Payments.Models;

namespace Ecommerce.Application.Features.Payments.Public;

public sealed class PaymentsService : IPaymentsService
{
    private readonly IOrderRepository _orders;
    private readonly IPaymentsGateway _gateway;

    public PaymentsService(IOrderRepository orders, IPaymentsGateway gateway)
    {
        _orders = orders;
        _gateway = gateway;
    }

    public async Task<Result<PaymentIntentResponse>> CreatePaymentIntentAsync(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var infoResult = await _orders.GetPaymentInfoByPublicIdAsync(publicId, cancellationToken);
        if (!infoResult.IsSuccess || infoResult.Data is null)
        {
            return infoResult.Status switch
            {
                ResultStatus.NotFound => Result<PaymentIntentResponse>.NotFound(),
                _ => Result<PaymentIntentResponse>.BadRequest(infoResult.Error ?? "Unable to load order.")
            };
        }

        if (infoResult.Data.Status != OrderStatus.PendingPayment)
        {
            return Result<PaymentIntentResponse>.Conflict("Order status is not PendingPayment.");
        }

        var amount = ToMinorUnits(infoResult.Data.TotalAmount);
        if (amount <= 0)
        {
            return Result<PaymentIntentResponse>.BadRequest("Order amount must be greater than zero.");
        }

        var intentResult = await _gateway.CreatePaymentIntentAsync(
            amount,
            infoResult.Data.Currency,
            infoResult.Data.PublicId,
            cancellationToken);

        if (!intentResult.IsSuccess || intentResult.Data is null)
        {
            return intentResult.Status switch
            {
                ResultStatus.BadRequest => Result<PaymentIntentResponse>.BadRequest(intentResult.Error ?? "Unable to create payment intent."),
                _ => Result<PaymentIntentResponse>.BadRequest(intentResult.Error ?? "Unable to create payment intent.")
            };
        }

        return Result<PaymentIntentResponse>.Ok(new PaymentIntentResponse(intentResult.Data));
    }

    public async Task<Result> HandleStripeWebhookAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken)
    {
        var eventResult = await _gateway.ParseWebhookEventAsync(payload, signatureHeader, cancellationToken);
        if (!eventResult.IsSuccess || eventResult.Data is null)
        {
            return eventResult.Status switch
            {
                ResultStatus.BadRequest => Result.BadRequest(eventResult.Error ?? "Invalid webhook."),
                _ => Result.BadRequest(eventResult.Error ?? "Invalid webhook.")
            };
        }

        if (!string.Equals(eventResult.Data.Type, "payment_intent.succeeded", StringComparison.Ordinal))
        {
            return Result.Ok();
        }

        if (string.IsNullOrWhiteSpace(eventResult.Data.OrderPublicId))
        {
            return Result.BadRequest("Missing orderPublicId metadata.");
        }

        if (!Guid.TryParse(eventResult.Data.OrderPublicId, out var publicId))
        {
            return Result.BadRequest("Invalid orderPublicId metadata.");
        }

        var updateResult = await _orders.MarkPaidByPublicIdAsync(publicId, cancellationToken);
        if (!updateResult.IsSuccess)
        {
            return updateResult.Status switch
            {
                ResultStatus.NotFound => Result.NotFound(),
                ResultStatus.Conflict => Result.Conflict(updateResult.Error ?? "Order status is not payable."),
                _ => Result.BadRequest(updateResult.Error ?? "Unable to update order status.")
            };
        }

        return Result.Ok();
    }

    private static long ToMinorUnits(decimal amount)
    {
        var scaled = decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        return decimal.ToInt64(scaled);
    }
}
