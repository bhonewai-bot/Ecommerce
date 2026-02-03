using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Payments.Public;
using Ecommerce.Application.Features.Idempotency;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController : ControllerBase
{
    private readonly IPaymentsService _payments;
    private readonly IdempotencyService _idempotency;

    public PaymentsController(IPaymentsService payments, IdempotencyService idempotency)
    {
        _payments = payments;
        _idempotency = idempotency;
    }

    [HttpPost("{publicId:guid}/intent")]
    public async Task<ActionResult<PaymentIntentResponse>> CreateIntent(
        Guid publicId,
        CancellationToken cancellationToken)
    {
        if (!HttpContext.Items.TryGetValue("Idempotency-Key", out var keyObj) || keyObj is not string key || string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Idempotency-Key header is required.");
        }

        var idempotencyResult = await _idempotency.ExecuteAsync(
            "payments.create_intent",
            key,
            new { publicId },
            ct => _payments.CreatePaymentIntentAsync(publicId, key, ct),
            200,
            cancellationToken);

        return idempotencyResult.Result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(idempotencyResult.Result.Error),
            ResultStatus.BadRequest => BadRequest(idempotencyResult.Result.Error),
            _ => StatusCode(idempotencyResult.StatusCode, idempotencyResult.Result.Data)
        };
    }
}
