using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Payments.Public;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Stripe;

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
                    new StripeWebhookEventDto(stripeEvent.Id, stripeEvent.Type, publicId, intent.Id)));
            }

            return Task.FromResult(Result<StripeWebhookEventDto>.Ok(
                new StripeWebhookEventDto(stripeEvent.Id, stripeEvent.Type, null, null)));
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
