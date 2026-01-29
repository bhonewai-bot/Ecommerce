using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Categories.Models;
using Ecommerce.Application.Features.Categories.Public;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Public;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
    private readonly IPublicCategoriesService _service;

    public CategoriesController(IPublicCategoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _service.GetAllAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);

        return item.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            _ => Ok(item.Data)
        };
    }

}
