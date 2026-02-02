using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Payments.Public;
using Microsoft.Extensions.Configuration;
using Stripe;

namespace Ecommerce.Infrastructure.Payments;

public sealed class StripePaymentsGateway : IPaymentsGateway
{
    private readonly IStripeClient _client;
    private readonly string _webhookSecret;

    public StripePaymentsGateway(IStripeClient client, IConfiguration configuration)
    {
        _client = client;
        _webhookSecret = configuration["Stripe:WebhookSecret"] ?? string.Empty;
    }

    public async Task<Result<StripePaymentIntentDto>> CreatePaymentIntentAsync(
        long amount,
        string currency,
        Guid orderPublicId,
        CancellationToken cancellationToken)
    {
        var service = new PaymentIntentService();
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

        var intent = await service.CreateAsync(options, cancellationToken: cancellationToken);
        if (string.IsNullOrWhiteSpace(intent.ClientSecret))
        {
            return Result<StripePaymentIntentDto>.BadRequest("Stripe client secret is missing.");
        }

        return Result<StripePaymentIntentDto>.Ok(
            new StripePaymentIntentDto(intent.Id, intent.ClientSecret));
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
            return Task.FromResult(Result<StripeWebhookEventDto>.BadRequest(ex.Message));
        }
    }
}
