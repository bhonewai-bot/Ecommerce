using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Payments.Models;
using Ecommerce.Application.Features.Payments.Public;
using Ecommerce.Application.Features.Idempotency;
using Ecommerce.WebApi.Errors;
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
            return this.ApiBadRequest("Idempotency-Key header is required.", ApiErrorCodes.IdempotencyKeyRequired);
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
            ResultStatus.NotFound => this.ApiNotFound(),
            ResultStatus.Conflict => this.ApiConflict(idempotencyResult.Result.Error),
            ResultStatus.BadRequest => this.ApiBadRequest(idempotencyResult.Result.Error),
            _ => StatusCode(idempotencyResult.StatusCode, idempotencyResult.Result.Data)
        };
    }
}
