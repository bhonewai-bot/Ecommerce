using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Checkout.Models;
using Ecommerce.Application.Features.Idempotency;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/checkout")]
public sealed class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;
    private readonly IdempotencyService _idempotency;

    public CheckoutController(ICheckoutService checkoutService, IdempotencyService idempotency)
    {
        _checkoutService = checkoutService;
        _idempotency = idempotency;
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> Create(
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        if (!HttpContext.Items.TryGetValue("Idempotency-Key", out var keyObj) || keyObj is not string key || string.IsNullOrWhiteSpace(key))
        {
            return BadRequest("Idempotency-Key header is required.");
        }

        var idempotencyResult = await _idempotency.ExecuteAsync(
            "checkout.create",
            key,
            request,
            ct => _checkoutService.CreateOrderAsync(request, ct),
            201,
            cancellationToken);

        return idempotencyResult.Result.Status switch
        {
            ResultStatus.BadRequest => BadRequest(idempotencyResult.Result.Error),
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(idempotencyResult.Result.Error),
            _ => StatusCode(idempotencyResult.StatusCode, idempotencyResult.Result.Data)
        };
    }
}
