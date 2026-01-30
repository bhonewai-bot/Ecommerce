using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Orders.Models;
using Ecommerce.Application.Features.Orders.Public;
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
    public async Task<ActionResult<GetOrderResponse>> GetByPublicId(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await _service.GetByPublicIdAsync(publicId, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            _ => Ok(result.Data)
        };
    }
}
