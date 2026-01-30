using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Public;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/webhooks/stripe")]
public sealed class StripeWebhooksController : ControllerBase
{
    private readonly IPaymentsService _payments;

    public StripeWebhooksController(IPaymentsService payments)
    {
        _payments = payments;
    }

    [HttpPost]
    public async Task<IActionResult> Handle(CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);
        var signature = Request.Headers["Stripe-Signature"].ToString();

        var result = await _payments.HandleStripeWebhookAsync(payload, signature, cancellationToken);

        return result.Status switch
        {
            ResultStatus.BadRequest => BadRequest(result.Error),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(result.Error),
            _ => Ok()
        };
    }
}
