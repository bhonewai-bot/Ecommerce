namespace Ecommerce.Application.Contracts;

public interface IIdempotencyStore
{
    Task<(bool StartedNew, IdempotencyRecord? Existing)> TryBeginAsync(
        string key,
        string scope,
        string requestHash,
        CancellationToken cancellationToken);

    Task CompleteAsync(
        string key,
        string scope,
        string status,
        int? responseCode,
        string? responseBodyJson,
        CancellationToken cancellationToken);
}

public sealed record IdempotencyRecord(
    string Key,
    string Scope,
    string RequestHash,
    string Status,
    int? ResponseCode,
    string? ResponseBodyJson);
