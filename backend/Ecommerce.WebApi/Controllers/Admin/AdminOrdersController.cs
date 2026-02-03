using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Checkout;
using Ecommerce.Application.Features.Idempotency;
using Ecommerce.Application.Features.Orders.Admin;
using Ecommerce.Application.Features.Orders.Models;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Admin;

[ApiController]
[Route("api/admin/orders")]
public sealed class AdminOrdersController : ControllerBase
{
    private readonly IAdminOrdersService _service;
    private readonly IdempotencyService _idempotency;

    public AdminOrdersController(IAdminOrdersService service, IdempotencyService idempotency)
    {
        _service = service;
        _idempotency = idempotency;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] OrderStatus? status = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(new AdminOrderListQuery
        {
            Page = page,
            PageSize = pageSize,
            Status = status
        }, cancellationToken);

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize,
            status = result.Status
        });
    }

    [HttpGet("{publicId:guid}")]
    public async Task<ActionResult<OrderDto>> GetByPublicId(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByPublicIdAsync(publicId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            _ => Ok(result.Data)
        };
    }

    [HttpPatch("{publicId:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid publicId,
        UpdateOrderStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (HttpContext.Items.TryGetValue("Idempotency-Key", out var keyObj) &&
            keyObj is string key &&
            !string.IsNullOrWhiteSpace(key))
        {
            var idempotencyResult = await _idempotency.ExecuteAsync(
                "admin.orders.update_status",
                key,
                new { publicId, request.Status },
                ct => _service.UpdateStatusAsync(publicId, request.Status, ct),
                204,
                cancellationToken);

            return idempotencyResult.Result.Status switch
            {
                ResultStatus.NotFound => NotFound(),
                ResultStatus.Conflict => Conflict(idempotencyResult.Result.Error),
                ResultStatus.BadRequest => BadRequest(idempotencyResult.Result.Error),
                _ => StatusCode(idempotencyResult.StatusCode)
            };
        }

        var result = await _service.UpdateStatusAsync(publicId, request.Status, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(result.Error),
            ResultStatus.BadRequest => BadRequest(result.Error),
            _ => NoContent()
        };
    }
}
