using Ecommerce.Application.Admin.Products;
using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers.Admin;

[ApiController]
[Route("api/admin/products")]
public sealed class AdminProductsController : ControllerBase
{
    private readonly IAdminProductsService _service;

    public AdminProductsController(IAdminProductsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string? q = null,
        [FromQuery] string sortBy = "id",
        [FromQuery] string sortOrder = "asc",
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(new GetAdminProductsParams
        {
            Page = page,
            PageSize = pageSize,
            CategoryId = categoryId,
            Query = q,
            SortBy = sortBy,
            SortOrder = sortOrder
        }, cancellationToken);

        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            page = result.Page,
            pageSize = result.PageSize,
            categoryId = result.CategoryId,
            sortBy = result.SortBy,
            sortOrder = result.SortOrder
        });
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _service.GetByIdAsync(id, cancellationToken);

        return item.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            _ => Ok(item.Data)
        };
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(dto, cancellationToken);

        return result.Status switch
        {
            ResultStatus.BadRequest => BadRequest(result.Error),
            _ => CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data)
        };
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateAsync(id, dto, cancellationToken);

        return result.Status switch
        {
            ResultStatus.NotFound => NotFound(),
            ResultStatus.BadRequest => BadRequest(result.Error),
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
            _ => NoContent()
        };
    }
}
