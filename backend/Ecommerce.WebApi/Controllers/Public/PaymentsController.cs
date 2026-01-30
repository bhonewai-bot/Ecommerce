using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Payments.Public;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _payments;

    public PaymentsController(IPaymentsService payments)
    {
        _payments = payments;
    }

    [HttpPost("{publicId:guid}/intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreateIntent(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        var result = await _payments.CreatePaymentIntentAsync(publicId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(result.Error),
            ResultStatus.BadRequest => BadRequest(result.Error),
            _ => Ok(result.Data)
        };
    }
}
