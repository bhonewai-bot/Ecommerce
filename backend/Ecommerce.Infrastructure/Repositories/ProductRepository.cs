using Ecommerce.Application.Common;
using Ecommerce.Application.Common.Dtos;
using Ecommerce.Application.Contracts;
using Ecommerce.Application.Products;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories;

public sealed class ProductRepository : IProductRepository
{
    private readonly EcommerceDbContext _db;

    public ProductRepository(EcommerceDbContext db)
    {
        _db = db;
    }

    public async Task<(IReadOnlyList<ProductDto> Items, int TotalCount)> GetProductsAsync(
        ProductListQuery query,
        CancellationToken cancellationToken)
    {
        var dbQuery = _db.products
            .AsNoTracking()
            .Where(p => !p.delete_flag && !p.category.delete_flag);

        if (query.CategoryId.HasValue)
        {
            dbQuery = dbQuery.Where(p => p.category_id == query.CategoryId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLowerInvariant();
            dbQuery = dbQuery.Where(p =>
                p.name.ToLower().Contains(term) ||
                (p.description != null && p.description.ToLower().Contains(term)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        dbQuery = ApplySorting(dbQuery, query.SortBy, query.SortOrder);

        var items = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ProductDto(
                p.id,
                p.category_id,
                p.name,
                p.description,
                p.price,
                p.image_url))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<IReadOnlyList<ProductDto>> GetProductsByIdsAsync(
        IReadOnlyCollection<int> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<ProductDto>();
        }

        return await _db.products
            .AsNoTracking()
            .Where(p => ids.Contains(p.id) && !p.delete_flag && !p.category.delete_flag)
            .Select(p => new ProductDto(
                p.id,
                p.category_id,
                p.name,
                p.description,
                p.price,
                p.image_url))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<ProductDto>> GetProductByIdAsync(int id, CancellationToken cancellationToken)
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

        return item is null ? Result<ProductDto>.NotFound() : Result<ProductDto>.Ok(item);
    }

    public async Task<Result<ProductDto>> CreateAsync(ProductCreateDto dto, CancellationToken cancellationToken)
    {
        var categoryExists = await _db.categories
            .AnyAsync(c => c.id == dto.CategoryId && !c.delete_flag, cancellationToken);

        if (!categoryExists)
        {
            return Result<ProductDto>.BadRequest("Category does not exist.");
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

        var dtoResult = new ProductDto(
            entity.id,
            entity.category_id,
            entity.name,
            entity.description,
            entity.price,
            entity.image_url);

        return Result<ProductDto>.Ok(dtoResult);
    }

    public async Task<Result> UpdateAsync(int id, ProductUpdateDto dto, CancellationToken cancellationToken)
    {
        var entity = await _db.products
            .FirstOrDefaultAsync(p => p.id == id && !p.delete_flag, cancellationToken);

        if (entity is null)
        {
            return Result.NotFound();
        }

        var categoryExists = await _db.categories
            .AnyAsync(c => c.id == dto.CategoryId && !c.delete_flag, cancellationToken);

        if (!categoryExists)
        {
            return Result.BadRequest("Category does not exist.");
        }

        entity.category_id = dto.CategoryId;
        entity.name = dto.Name;
        entity.description = dto.Description;
        entity.price = dto.Price;
        entity.image_url = dto.ImageUrl;

        await _db.SaveChangesAsync(cancellationToken);
        return Result.Ok();
    }

    public async Task<Result> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var entity = await _db.products
            .FirstOrDefaultAsync(p => p.id == id && !p.delete_flag, cancellationToken);

        if (entity is null)
        {
            return Result.NotFound();
        }

        entity.delete_flag = true;
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Ok();
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
}
