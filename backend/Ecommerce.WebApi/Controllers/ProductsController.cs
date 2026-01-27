using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Ecommerce.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ProductsController : ControllerBase
{
    private readonly EcommerceDbContext _db;

    public ProductsController(EcommerceDbContext db)
    {
        _db = db;
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
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var query = _db.products
            .AsNoTracking()
            .Where(p => !p.delete_flag && !p.category.delete_flag);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.category_id == categoryId.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        query = ApplySorting(query, sortBy, sortOrder);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductDto(
                p.id,
                p.category_id,
                p.name,
                p.description,
                p.price,
                p.image_url))
            .ToListAsync(cancellationToken);

        return Ok(new
        {
            items,
            totalCount,
            page,
            pageSize,
            categoryId,
            sortBy,
            sortOrder
        });
    }

    private static IQueryable<product> ApplySorting(
        IQueryable<product> query,
        string sortBy,
        string sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        var key = sortBy?.ToLowerInvariant();

        return key switch
        {
            "name" => desc ? query.OrderByDescending(p => p.name) : query.OrderBy(p => p.name),
            "price" => desc ? query.OrderByDescending(p => p.price) : query.OrderBy(p => p.price),
            _ => desc ? query.OrderByDescending(p => p.id) : query.OrderBy(p => p.id)
        };
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _db.products
            .AsNoTracking()
            .Where(p => p.id == id && !p.delete_flag && !p.category.delete_flag)
            .Select(p => new ProductDto(
                p.id,
                p.category_id,
                p.name,
                p.description,
                p.price,
                p.image_url))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> Create(ProductCreateDto dto, CancellationToken cancellationToken)
    {
        var categoryExists = await _db.categories
            .AnyAsync(c => c.id == dto.CategoryId && !c.delete_flag, cancellationToken);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist.");
        }

        var entity = new product
        {
            category_id = dto.CategoryId,
            name = dto.Name,
            description = dto.Description,
            price = dto.Price,
            image_url = dto.ImageUrl,
            delete_flag = false
        };

        _db.products.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var result = new ProductDto(
            entity.id,
            entity.category_id,
            entity.name,
            entity.description,
            entity.price,
            entity.image_url);

        return CreatedAtAction(nameof(GetById), new { id = entity.id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, ProductUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _db.products
            .FirstOrDefaultAsync(p => p.id == id && !p.delete_flag, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var categoryExists = await _db.categories
            .AnyAsync(c => c.id == dto.CategoryId && !c.delete_flag, cancellationToken);

        if (!categoryExists)
        {
            return BadRequest("Category does not exist.");
        }

        entity.category_id = dto.CategoryId;
        entity.name = dto.Name;
        entity.description = dto.Description;
        entity.price = dto.Price;
        entity.image_url = dto.ImageUrl;

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.products
            .FirstOrDefaultAsync(p => p.id == id && !p.delete_flag, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        entity.delete_flag = true;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
