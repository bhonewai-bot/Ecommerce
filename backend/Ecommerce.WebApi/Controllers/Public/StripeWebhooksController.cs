using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Public;
using Ecommerce.WebApi.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/webhooks/stripe")]
public sealed class StripeWebhooksController : ControllerBase
{
    private readonly IPaymentsService _payments;
    private readonly ILogger<StripeWebhooksController> _logger;

    public StripeWebhooksController(IPaymentsService payments, ILogger<StripeWebhooksController> logger)
    {
        _payments = payments;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stripe webhook endpoint hit.");
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _payments.HandleStripeWebhookAsync(payload, signature, cancellationToken);

        return result.Status switch
        {
            ResultStatus.BadRequest => this.ApiBadRequest(result.Error),
            ResultStatus.NotFound => this.ApiNotFound(),
            ResultStatus.Conflict => this.ApiConflict(result.Error),
            _ => Ok()
        };
    }
}
