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
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _db.products
            .AsNoTracking()
            .Where(p => !p.delete_flag && !p.category.delete_flag)
            .Select(p => new ProductDto(
                p.id,
                p.category_id,
                p.name,
                p.description,
                p.price,
                p.image_url))
            .ToListAsync(cancellationToken);

        return Ok(items);
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
