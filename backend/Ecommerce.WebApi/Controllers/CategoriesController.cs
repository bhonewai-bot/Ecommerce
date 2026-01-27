using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Ecommerce.WebApi.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class CategoriesController : ControllerBase
{
    private readonly EcommerceDbContext _db;

    public CategoriesController(EcommerceDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await _db.categories
            .AsNoTracking()
            .Where(c => !c.delete_flag)
            .Select(c => new CategoryDto(c.id, c.name, c.description))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CategoryDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _db.categories
            .AsNoTracking()
            .Where(c => c.id == id && !c.delete_flag)
            .Select(c => new CategoryDto(c.id, c.name, c.description))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
        {
            return NotFound();
        }

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDto>> Create(CategoryCreateDto dto, CancellationToken cancellationToken)
    {
        var entity = new category
        {
            name = dto.Name,
            description = dto.Description,
            delete_flag = false
        };

        _db.categories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        var result = new CategoryDto(entity.id, entity.name, entity.description);
        return CreatedAtAction(nameof(GetById), new { id = entity.id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _db.categories
            .FirstOrDefaultAsync(c => c.id == id && !c.delete_flag, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        entity.name = dto.Name;
        entity.description = dto.Description;

        await _db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.categories
            .FirstOrDefaultAsync(c => c.id == id && !c.delete_flag, cancellationToken);

        if (entity is null)
        {
            return NotFound();
        }

        var hasActiveProducts = await _db.products
            .AnyAsync(p => p.category_id == id && !p.delete_flag, cancellationToken);

        if (hasActiveProducts)
        {
            return Conflict("Category has active products. Delete products first.");
        }

        entity.delete_flag = true;
        await _db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}
