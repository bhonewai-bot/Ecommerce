using Ecommerce.Application.Admin.Categories;
using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Admin;

[ApiController]
[Route("api/admin/categories")]
public sealed class AdminCategoriesController : ControllerBase
{
    private readonly IAdminCategoriesService _service;

    public AdminCategoriesController(IAdminCategoriesService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? q = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(new GetAdminCategoriesParams
        {
            Page = page,
            PageSize = pageSize,
            Query = q
        }, cancellationToken);

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize,
            query = result.Query
        });
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

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        return result.Status switch
        {
            _ => CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
        };
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            _ => NoContent()
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.Conflict => Conflict(result.Error),
            _ => NoContent()
        };
    }
}
