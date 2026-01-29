using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Contracts;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly EcommerceDbContext _db;

    public CategoryRepository(EcommerceDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<CategoryDto> Items, int TotalCount)> GetCategoriesAsync(
        int page,
        int pageSize,
        string? query,
        CancellationToken cancellationToken)
    {
        var dbQuery = _db.categories
            .AsNoTracking()
            .Where(c => !c.delete_flag);

        if (!string.IsNullOrWhiteSpace(query))
        {
            var term = query.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(c =>
                c.name.ToLower().Contains(term) ||
                (c.description != null && c.description.ToLower().Contains(term)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(c => c.id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CategoryDto(c.id, c.name, c.description))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Result<CategoryDto>> GetCategoryByIdAsync(int id, CancellationToken cancellationToken)
    {
        var item = await _db.categories
            .AsNoTracking()
            .Where(c => c.id == id && !c.delete_flag)
            .Select(c => new CategoryDto(c.id, c.name, c.description))
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? Result<CategoryDto>.NotFound() : Result<CategoryDto>.Ok(item);
    }

    public async Task<Result<CategoryDto>> CreateAsync(CategoryCreateDto dto, CancellationToken cancellationToken)
    {
        var entity = new category
        {
            name = dto.Name,
            description = dto.Description,
            delete_flag = false
        };

        _db.categories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Ok(new CategoryDto(entity.id, entity.name, entity.description));
    }

    public async Task<Result> UpdateAsync(int id, CategoryUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _db.categories
            .FirstOrDefaultAsync(c => c.id == id && !c.delete_flag, cancellationToken);

        if (entity is null)
        {
            return Result.NotFound();
        }

        entity.name = dto.Name;
        entity.description = dto.Description;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var loaded = await _db.categories
            .Where(c => c.id == id && !c.delete_flag)
            .Select(c => new
            {
                Entity = c,
                HasActiveProducts = c.products.Any(p => !p.delete_flag)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (loaded is null)
        {
            return Result.NotFound();
        }

        if (loaded.HasActiveProducts)
        {
            return Result.Conflict("Category has active products. Delete products first.");
        }

        loaded.Entity.delete_flag = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<IReadOnlyList<CategoryDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _db.categories
            .AsNoTracking()
            .Where(c => !c.delete_flag)
            .Select(c => new CategoryDto(c.id, c.name, c.description))
            .ToListAsync(cancellationToken);
    }
}
