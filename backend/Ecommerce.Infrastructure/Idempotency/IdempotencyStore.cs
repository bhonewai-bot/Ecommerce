using Ecommerce.Application.Contracts;
using Ecommerce.Infrastructure.Data;
using Ecommerce.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Idempotency;

public sealed class IdempotencyStore : IIdempotencyStore
{
    private readonly EcommerceDbContext _db;

    public IdempotencyStore(EcommerceDbContext db)
    {
        _db = db;
    }

    public async Task<(bool StartedNew, IdempotencyRecord? Existing)> TryBeginAsync(
        string key,
        string scope,
        string requestHash,
        CancellationToken cancellationToken)
    {
        var rows = await _db.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO idempotency_keys
                (idempotency_key, scope, request_hash, status)
            VALUES
                ({key}, {scope}, {requestHash}, {"processing"})
            ON CONFLICT (idempotency_key, scope) DO NOTHING
        ", cancellationToken);

        if (rows == 1)
        {
            return (true, null);
        }

        var existing = await _db.idempotency_keys
            .AsNoTracking()
            .Where(e => e.idempotency_key_value == key && e.scope == scope)
            .Select(e => new IdempotencyRecord(
                e.idempotency_key_value,
                e.scope,
                e.request_hash,
                e.status,
                e.response_code,
                e.response_body))
            .FirstOrDefaultAsync(cancellationToken);

        return (false, existing);
    }

    public async Task CompleteAsync(
        string key,
        string scope,
        string status,
        int? responseCode,
        string? responseBodyJson,
        CancellationToken cancellationToken)
    {
        var entity = await _db.idempotency_keys
            .FirstOrDefaultAsync(e => e.idempotency_key_value == key && e.scope == scope, cancellationToken);

        if (entity is null)
        {
            return;
        }

        entity.status = status;
        entity.response_code = responseCode;
        entity.response_body = responseBodyJson;
        entity.completed_at = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);
    }
}
