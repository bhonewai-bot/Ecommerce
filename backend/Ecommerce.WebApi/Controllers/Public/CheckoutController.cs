using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Checkout.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/checkout")]
public sealed class CheckoutController : ControllerBase
{
    private readonly ICheckoutService _checkoutService;

    public CheckoutController(ICheckoutService checkoutService)
    {
        _checkoutService = checkoutService;
    }

    [HttpPost]
    public async Task<ActionResult<CheckoutResponse>> Create(
        CheckoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _checkoutService.CreateOrderAsync(request, cancellationToken);

        return result.Status switch
        {
            ResultStatus.BadRequest => BadRequest(result.Error),
            ResultStatus.NotFound => NotFound(),
            _ => Created(string.Empty, result.Data)
        };
    }
}
