using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Payments.Public;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;

namespace Ecommerce.Infrastructure.Payments;

public sealed class StripePaymentsGateway : IPaymentsGateway
{
    private readonly IStripeClient _client;
    private readonly string _webhookSecret;
    private readonly ILogger<StripePaymentsGateway> _logger;

    public StripePaymentsGateway(
        IStripeClient client,
        IConfiguration configuration,
        ILogger<StripePaymentsGateway> logger)
    {
        _client = client;
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
        _logger = logger;
    }

    public async Task<Result<StripePaymentIntentDto>> CreatePaymentIntentAsync(
        long amount,
        string currency,
        Guid orderPublicId,
        string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var service = new PaymentIntentService(_client);
        var requestOptions = string.IsNullOrWhiteSpace(idempotencyKey)
            ? null
            : new RequestOptions { IdempotencyKey = idempotencyKey };
        var options = new PaymentIntentCreateOptions
        {
            Amount = amount,
            Currency = currency,
            AutomaticPaymentMethods = null,
            PaymentMethodTypes = new List<string> { "card" },
            Metadata = new Dictionary<string, string>
            {
                ["orderPublicId"] = orderPublicId.ToString()
            }
        };

        try
        {
            var intent = await service.CreateAsync(options, requestOptions, cancellationToken);
            if (string.IsNullOrWhiteSpace(intent.ClientSecret))
            {
                return Result<StripePaymentIntentDto>.BadRequest("Payment service is misconfigured.");
            }

            return Result<StripePaymentIntentDto>.Ok(
                new StripePaymentIntentDto(intent.Id, intent.ClientSecret));
        }
        catch (StripeException ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["HttpStatusCode"] = ex.HttpStatusCode,
                       ["StripeErrorCode"] = ex.StripeError?.Code,
                       ["StripeErrorMessage"] = ex.StripeError?.Message,
                       ["StripeRequestId"] = ex.StripeResponse?.RequestId
                   }))
            {
                _logger.LogError(ex, "Stripe payment intent creation failed");
            }

            return Result<StripePaymentIntentDto>.BadRequest("Payment service is misconfigured.");
        }
    }

    public async Task<Result<StripeCheckoutSessionDto>> CreateCheckoutSessionAsync(
        StripeCheckoutSessionRequest request,
        CancellationToken cancellationToken)
    {
        var service = new SessionService(_client);
        var orderPublicId = request.OrderPublicId.ToString();
        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            CustomerEmail = string.IsNullOrWhiteSpace(request.CustomerEmail) ? null : request.CustomerEmail,
            ClientReferenceId = orderPublicId,
            Metadata = new Dictionary<string, string>
            {
                ["orderPublicId"] = orderPublicId
            },
            PaymentIntentData = new SessionPaymentIntentDataOptions
            {
                Metadata = new Dictionary<string, string>
                {
                    ["orderPublicId"] = orderPublicId
                }
            },
            LineItems = request.LineItems.Select(item => new SessionLineItemOptions
            {
                Quantity = item.Quantity,
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = request.Currency.ToLowerInvariant(),
                    UnitAmount = item.UnitAmount,
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = item.Name
                    }
                }
            }).ToList()
        };

        try
        {
            var session = await service.CreateAsync(options, cancellationToken: cancellationToken);
            if (string.IsNullOrWhiteSpace(session.Url))
            {
                return Result<StripeCheckoutSessionDto>.BadRequest("Payment service is misconfigured.");
            }

            return Result<StripeCheckoutSessionDto>.Ok(
                new StripeCheckoutSessionDto(session.Id, session.Url, request.OrderPublicId));
        }
        catch (StripeException ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["HttpStatusCode"] = ex.HttpStatusCode,
                       ["StripeErrorCode"] = ex.StripeError?.Code,
                       ["StripeErrorMessage"] = ex.StripeError?.Message,
                       ["StripeRequestId"] = ex.StripeResponse?.RequestId
                   }))
            {
                _logger.LogError(ex, "Stripe checkout session creation failed");
            }

            return Result<StripeCheckoutSessionDto>.BadRequest("Payment service is misconfigured.");
        }
    }

    public async Task<Result<StripeCheckoutSessionDto>> GetCheckoutSessionAsync(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var service = new SessionService(_client);

        try
        {
            var session = await service.GetAsync(sessionId, cancellationToken: cancellationToken);
            var publicIdValue = session.Metadata?.TryGetValue("orderPublicId", out var publicId) == true
                ? publicId
                : session.ClientReferenceId;

            Guid? parsedPublicId = null;
            if (Guid.TryParse(publicIdValue, out var parsed))
            {
                parsedPublicId = parsed;
            }

            return Result<StripeCheckoutSessionDto>.Ok(
                new StripeCheckoutSessionDto(session.Id, session.Url, parsedPublicId));
        }
        catch (StripeException ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["HttpStatusCode"] = ex.HttpStatusCode,
                       ["StripeErrorCode"] = ex.StripeError?.Code,
                       ["StripeErrorMessage"] = ex.StripeError?.Message,
                       ["StripeRequestId"] = ex.StripeResponse?.RequestId
                   }))
            {
                _logger.LogError(ex, "Stripe checkout session fetch failed");
            }

            return Result<StripeCheckoutSessionDto>.BadRequest("Unable to load checkout session.");
        }
    }

    public Task<Result<StripeWebhookEventDto>> ParseWebhookEventAsync(
        string payload,
        string signatureHeader,
        CancellationToken cancellationToken)
    {
        try
        {
            var stripeEvent = EventUtility.ConstructEvent(
                payload,
                signatureHeader,
                _webhookSecret,
                throwOnApiVersionMismatch: false);

            if (stripeEvent.Data.Object is PaymentIntent intent)
            {
                intent.Metadata.TryGetValue("orderPublicId", out var publicId);
                return Task.FromResult(Result<StripeWebhookEventDto>.Ok(
                    new StripeWebhookEventDto(stripeEvent.Id, stripeEvent.Type, publicId, intent.Id, null)));
            }

            if (stripeEvent.Data.Object is Session session)
            {
                var publicId = session.Metadata?.TryGetValue("orderPublicId", out var value) == true
                    ? value
                    : session.ClientReferenceId;
                return Task.FromResult(Result<StripeWebhookEventDto>.Ok(
                    new StripeWebhookEventDto(
                        stripeEvent.Id,
                        stripeEvent.Type,
                        publicId,
                        session.PaymentIntentId,
                        session.Id)));
            }

            return Task.FromResult(Result<StripeWebhookEventDto>.Ok(
                new StripeWebhookEventDto(stripeEvent.Id, stripeEvent.Type, null, null, null)));
        }
        catch (StripeException ex)
        {
            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["HttpStatusCode"] = ex.HttpStatusCode,
                       ["StripeErrorCode"] = ex.StripeError?.Code,
                       ["StripeErrorMessage"] = ex.StripeError?.Message,
                       ["StripeRequestId"] = ex.StripeResponse?.RequestId
                   }))
            {
                _logger.LogError(ex, "Stripe webhook parse failed");
            }

            return Task.FromResult(Result<StripeWebhookEventDto>.BadRequest("Invalid webhook."));
        }
    }
}
