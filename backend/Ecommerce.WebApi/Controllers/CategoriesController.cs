using Ecommerce.Infrastructure.Data;
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

}
