using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Public.Products;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly IPublicProductsService _service;

    public ProductsController(IPublicProductsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? categoryId = null,
        [FromQuery] string sortBy = "id",
        [FromQuery] string sortOrder = "asc",
        CancellationToken cancellationToken = default)
    {
        var result = await _service.GetAllAsync(
            page,
            pageSize,
            categoryId,
            sortBy,
            sortOrder,
            cancellationToken);

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

}
