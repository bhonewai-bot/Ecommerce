using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Features.Categories.Models;
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
        CategoryListQuery query,
        CancellationToken cancellationToken)
    {
        var dbQuery = _db.categories
            .AsNoTracking()
            .Where(c => !c.delete_flag);

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(c =>
                c.name.ToLower().Contains(term) ||
                (c.description != null && c.description.ToLower().Contains(term)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(c => c.id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
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

    public async Task<Result<CategoryDto>> CreateAsync(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        var entity = new category
        {
            name = command.Name,
            description = command.Description,
            delete_flag = false
        };

        _db.categories.Add(entity);
        await _db.SaveChangesAsync(cancellationToken);

        return Result<CategoryDto>.Ok(new CategoryDto(entity.id, entity.name, entity.description));
    }

    public async Task<Result> UpdateAsync(int id, UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var entity = await _db.categories
            .FirstOrDefaultAsync(c => c.id == id && !c.delete_flag, cancellationToken);

        if (entity is null)
        {
            return Result.NotFound();
        }

        entity.name = command.Name;
        entity.description = command.Description;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.categories
            .FirstOrDefaultAsync(c => c.id == id && !c.delete_flag, cancellationToken);

        if (entity is null)
        {
            return Result.NotFound();
        }

        entity.delete_flag = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    public async Task<bool> HasActiveProductsAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.products
            .AnyAsync(p => p.category_id == id && !p.delete_flag, cancellationToken);
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
