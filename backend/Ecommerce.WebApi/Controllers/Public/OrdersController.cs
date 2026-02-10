using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Orders.Models;
using Ecommerce.Application.Features.Orders.Public;
using Ecommerce.WebApi.Errors;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public sealed class OrdersController : ControllerBase
{
    private readonly IPublicOrdersService _service;

    public OrdersController(IPublicOrdersService service)
    {
        _service = service;
    }

    [HttpGet("{publicId:guid}")]
    public async Task<ActionResult<OrderDto>> GetByPublicId(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByPublicIdAsync(publicId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            _ => Ok(result.Data)
        };
    }

    [HttpGet("by-public-id/{publicId:guid}")]
    public async Task<ActionResult<OrderDto>> GetByPublicIdAlias(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByPublicIdAsync(publicId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            _ => Ok(result.Data)
        };
    }

    [HttpGet("by-checkout-session/{sessionId}")]
    public async Task<ActionResult<OrderDto>> GetByCheckoutSession(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetByCheckoutSessionIdAsync(sessionId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            ResultStatus.Conflict => this.ApiConflict(result.Error),
            ResultStatus.BadRequest => this.ApiBadRequest(result.Error),
            _ => Ok(result.Data)
        };
    }

    [HttpGet("by-checkout-session/{sessionId}/resume")]
    public async Task<ActionResult<CheckoutResumeDto>> GetResumableCheckoutBySession(
        string sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetResumableCheckoutBySessionIdAsync(sessionId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            ResultStatus.Conflict => this.ApiConflict(result.Error),
            ResultStatus.BadRequest => this.ApiBadRequest(result.Error),
            _ => Ok(result.Data)
        };
    }

    [HttpPost("{publicId:guid}/cancel")]
    public async Task<ActionResult<OrderDto>> Cancel(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _service.CancelPendingAsync(publicId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            ResultStatus.Conflict => this.ApiConflict(result.Error),
            ResultStatus.BadRequest => this.ApiBadRequest(result.Error),
            _ => Ok(result.Data)
        };
    }
}
