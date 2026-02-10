using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Payments.Models;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Features.Payments.Public;

public sealed class PaymentsService : IPaymentsService
{
    private readonly IOrderRepository _orders;
    private readonly IPaymentsGateway _gateway;
    private readonly IStripeEventDeduper _deduper;
    private readonly ILogger<PaymentsService> _logger;

    public PaymentsService(
        IOrderRepository orders,
        IPaymentsGateway gateway,
        IStripeEventDeduper deduper,
        ILogger<PaymentsService> logger)
    {
        _orders = orders;
        _gateway = gateway;
        _deduper = deduper;
        _logger = logger;
    }

    public async Task<Result<PaymentIntentResponse>> CreatePaymentIntentAsync(
        Guid publicId,
        string? idempotencyKey,
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

        if (infoResult.Data.HasCheckoutSession)
        {
            return Result<PaymentIntentResponse>.Conflict(
                "This order uses hosted checkout. Continue payment from Checkout Session.");
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
            idempotencyKey,
            cancellationToken);

        if (!intentResult.IsSuccess || intentResult.Data is null)
        {
            return intentResult.Status switch
            {
                ResultStatus.BadRequest => Result<PaymentIntentResponse>.BadRequest(intentResult.Error ?? "Unable to create payment intent."),
                _ => Result<PaymentIntentResponse>.BadRequest(intentResult.Error ?? "Unable to create payment intent.")
            };
        }

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["OrderPublicId"] = infoResult.Data.PublicId,
                   ["PaymentIntentId"] = intentResult.Data.PaymentIntentId,
                   ["Amount"] = amount,
                   ["Currency"] = infoResult.Data.Currency,
                   ["Source"] = "public_api"
               }))
        {
            _logger.LogInformation(
                "payment.intent.created {@Audit}",
                new
                {
                    OrderPublicId = infoResult.Data.PublicId,
                    PaymentIntentId = intentResult.Data.PaymentIntentId,
                    Amount = amount,
                    Currency = infoResult.Data.Currency,
                    Source = "public_api"
                });
        }

        return Result<PaymentIntentResponse>.Ok(new PaymentIntentResponse(intentResult.Data.ClientSecret));
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

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["StripeEventId"] = eventResult.Data.StripeEventId,
                   ["EventType"] = eventResult.Data.Type,
                   ["OrderPublicId"] = eventResult.Data.OrderPublicId,
                   ["PaymentIntentId"] = eventResult.Data.PaymentIntentId,
                   ["CheckoutSessionId"] = eventResult.Data.CheckoutSessionId,
                   ["Source"] = "webhook"
               }))
        {
            _logger.LogInformation(
                "stripe.webhook_received {@Audit}",
                new
                {
                    StripeEventId = eventResult.Data.StripeEventId,
                    EventType = eventResult.Data.Type,
                    OrderPublicId = eventResult.Data.OrderPublicId,
                    PaymentIntentId = eventResult.Data.PaymentIntentId,
                    CheckoutSessionId = eventResult.Data.CheckoutSessionId,
                    Source = "webhook"
                });
        }

        Guid? parsedOrderPublicId = null;
        if (Guid.TryParse(eventResult.Data.OrderPublicId, out var parsedValue))
        {
            parsedOrderPublicId = parsedValue;
        }

        var isFirstTime = await _deduper.TryMarkProcessedAsync(
            eventResult.Data.StripeEventId,
            eventResult.Data.Type,
            parsedOrderPublicId,
            eventResult.Data.PaymentIntentId,
            cancellationToken);

        if (!isFirstTime)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["StripeEventId"] = eventResult.Data.StripeEventId,
                       ["EventType"] = eventResult.Data.Type,
                       ["CheckoutSessionId"] = eventResult.Data.CheckoutSessionId,
                       ["Source"] = "webhook"
                   }))
            {
                _logger.LogInformation(
                    "stripe.webhook_duplicate_ignored {@Audit}",
                    new
                    {
                        StripeEventId = eventResult.Data.StripeEventId,
                        EventType = eventResult.Data.Type,
                        CheckoutSessionId = eventResult.Data.CheckoutSessionId,
                        Source = "webhook"
                    });
            }

            return Result.Ok();
        }

        if (!string.Equals(eventResult.Data.Type, "payment_intent.succeeded", StringComparison.Ordinal) &&
            !string.Equals(eventResult.Data.Type, "checkout.session.completed", StringComparison.Ordinal))
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

        var statusResult = await _orders.GetStatusByPublicIdAsync(publicId, cancellationToken);
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

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["OrderPublicId"] = publicId,
                   ["PaymentIntentId"] = eventResult.Data.PaymentIntentId,
                   ["CheckoutSessionId"] = eventResult.Data.CheckoutSessionId,
                   ["Source"] = "webhook"
               }))
        {
            _logger.LogInformation(
                "order.paid {@Audit}",
                new
                {
                    OrderPublicId = publicId,
                    PaymentIntentId = eventResult.Data.PaymentIntentId,
                    CheckoutSessionId = eventResult.Data.CheckoutSessionId,
                    Source = "webhook"
                });
        }

        if (statusResult.IsSuccess && statusResult.Data != OrderStatus.Paid)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["OrderPublicId"] = publicId,
                       ["OldStatus"] = statusResult.Data,
                       ["NewStatus"] = OrderStatus.Paid,
                       ["PaymentIntentId"] = eventResult.Data.PaymentIntentId,
                       ["CheckoutSessionId"] = eventResult.Data.CheckoutSessionId,
                       ["Source"] = "webhook"
                   }))
            {
                _logger.LogInformation(
                    "order.status_changed {@Audit}",
                    new
                    {
                        OrderPublicId = publicId,
                        OldStatus = statusResult.Data,
                        NewStatus = OrderStatus.Paid,
                        PaymentIntentId = eventResult.Data.PaymentIntentId,
                        CheckoutSessionId = eventResult.Data.CheckoutSessionId,
                        Source = "webhook"
                    });
            }
        }

        return Result.Ok();
    }

    private static long ToMinorUnits(decimal amount)
    {
        var scaled = decimal.Round(amount * 100m, 0, MidpointRounding.AwayFromZero);
        return decimal.ToInt64(scaled);
    }
}
