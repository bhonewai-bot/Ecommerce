using Ecommerce.Application.Common;
using Ecommerce.Application.Features.Categories.Admin;
using Ecommerce.Application.Features.Categories.Models;
using Ecommerce.WebApi.Errors;
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
        var result = await _service.GetAllAsync(new AdminCategoryListQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = q
        }, cancellationToken);

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize,
            query = result.Search
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);

        return item.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            _ => Ok(item.Data)
        };
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(request, cancellationToken);

        return result.Status switch
        {
            ResultStatus.BadRequest => this.ApiBadRequest(result.Error),
            ResultStatus.Conflict => this.ApiConflict(result.Error),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
        };
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, request, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            ResultStatus.BadRequest => this.ApiBadRequest(result.Error),
            _ => NoContent()
        };
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _service.DeleteAsync(id, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => this.ApiNotFound(),
            ResultStatus.Conflict => this.ApiConflict(result.Error),
            _ => NoContent()
        };
    }
}
