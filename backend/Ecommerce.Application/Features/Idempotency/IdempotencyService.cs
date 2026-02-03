using System.Text.Json;
using Ecommerce.Application.Common;
using Ecommerce.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.Features.Idempotency;

public sealed class IdempotencyService
{
    private readonly IIdempotencyStore _store;
    private readonly ILogger<IdempotencyService> _logger;

    public IdempotencyService(IIdempotencyStore store, ILogger<IdempotencyService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<IdempotencyResult<TResponse>> ExecuteAsync<TRequest, TResponse>(
        string scope,
        string key,
        TRequest request,
        Func<CancellationToken, Task<Result<TResponse>>> handlerFunc,
        int successStatusCode,
        CancellationToken cancellationToken)
    {
        var requestHash = IdempotencyHasher.HashRequest(request);

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["IdempotencyKey"] = key,
                   ["Scope"] = scope,
                   ["Outcome"] = "begin"
               }))
        {
            var (startedNew, existing) = await _store.TryBeginAsync(key, scope, requestHash, cancellationToken);

            if (!startedNew && existing is not null)
            {
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    using (_logger.BeginScope(new Dictionary<string, object?>
                           {
                               ["Outcome"] = "conflict"
                           }))
                    {
                        _logger.LogInformation("idempotency.conflict");
                    }
                    return new IdempotencyResult<TResponse>(
                        Result<TResponse>.Conflict("Idempotency key reuse with different request."),
                        409);
                }

                if (existing.ResponseCode.HasValue)
                {
                    using (_logger.BeginScope(new Dictionary<string, object?>
                           {
                               ["Outcome"] = "cache_hit"
                           }))
                    {
                        _logger.LogInformation("idempotency.cache_hit");
                    }

                    return existing.ResponseCode.Value switch
                    {
                        200 or 201 => new IdempotencyResult<TResponse>(
                            Result<TResponse>.Ok(JsonSerializer.Deserialize<TResponse>(existing.ResponseBodyJson!)!),
                            existing.ResponseCode.Value),
                        204 => new IdempotencyResult<TResponse>(Result<TResponse>.Ok(default!), 204),
                        404 => new IdempotencyResult<TResponse>(Result<TResponse>.NotFound(), 404),
                        409 => new IdempotencyResult<TResponse>(
                            Result<TResponse>.Conflict(JsonSerializer.Deserialize<string>(existing.ResponseBodyJson!) ?? "Conflict"),
                            409),
                        _ => new IdempotencyResult<TResponse>(
                            Result<TResponse>.BadRequest(JsonSerializer.Deserialize<string>(existing.ResponseBodyJson!) ?? "Bad request"),
                            existing.ResponseCode.Value)
                    };
                }

                using (_logger.BeginScope(new Dictionary<string, object?>
                       {
                           ["Outcome"] = "processing"
                       }))
                {
                    _logger.LogInformation("idempotency.processing");
                }
                return new IdempotencyResult<TResponse>(
                    Result<TResponse>.Conflict("Request is already processing"),
                    409);
            }

            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["Outcome"] = "started"
                   }))
            {
                _logger.LogInformation("idempotency.started");
            }

            var result = await handlerFunc(cancellationToken);

            var statusCode = result.Status switch
            {
                ResultStatus.Success => successStatusCode,
                ResultStatus.NotFound => 404,
                ResultStatus.Conflict => 409,
                _ => 400
            };

            string? responseJson = null;
            if (result.IsSuccess)
            {
                if (result.Data is not null)
                {
                    responseJson = JsonSerializer.Serialize(result.Data);
                }
            }
            else if (!string.IsNullOrWhiteSpace(result.Error))
            {
                responseJson = JsonSerializer.Serialize(result.Error);
            }

            var status = result.IsSuccess ? "completed" : "failed";
            await _store.CompleteAsync(key, scope, status, statusCode, responseJson, cancellationToken);

            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["Outcome"] = status
                   }))
            {
                _logger.LogInformation("idempotency.completed");
            }

            return new IdempotencyResult<TResponse>(result, statusCode);
        }
    }

    public async Task<IdempotencyResult<object>> ExecuteAsync<TRequest>(
        string scope,
        string key,
        TRequest request,
        Func<CancellationToken, Task<Result>> handlerFunc,
        int successStatusCode,
        CancellationToken cancellationToken)
    {
        var requestHash = IdempotencyHasher.HashRequest(request);

        using (_logger.BeginScope(new Dictionary<string, object?>
               {
                   ["IdempotencyKey"] = key,
                   ["Scope"] = scope,
                   ["Outcome"] = "begin"
               }))
        {
            var (startedNew, existing) = await _store.TryBeginAsync(key, scope, requestHash, cancellationToken);

            if (!startedNew && existing is not null)
            {
                if (!string.Equals(existing.RequestHash, requestHash, StringComparison.Ordinal))
                {
                    using (_logger.BeginScope(new Dictionary<string, object?>
                           {
                               ["Outcome"] = "conflict"
                           }))
                    {
                        _logger.LogInformation("idempotency.conflict");
                    }
                    return new IdempotencyResult<object>(
                        Result<object>.Conflict("Idempotency key reuse with different request."),
                        409);
                }

                if (existing.ResponseCode.HasValue)
                {
                    using (_logger.BeginScope(new Dictionary<string, object?>
                           {
                               ["Outcome"] = "cache_hit"
                           }))
                    {
                        _logger.LogInformation("idempotency.cache_hit");
                    }

                    return existing.ResponseCode.Value switch
                    {
                        204 or 200 => new IdempotencyResult<object>(Result<object>.Ok(new object()), existing.ResponseCode.Value),
                        404 => new IdempotencyResult<object>(Result<object>.NotFound(), 404),
                        409 => new IdempotencyResult<object>(
                            Result<object>.Conflict(JsonSerializer.Deserialize<string>(existing.ResponseBodyJson!) ?? "Conflict"),
                            409),
                        _ => new IdempotencyResult<object>(
                            Result<object>.BadRequest(JsonSerializer.Deserialize<string>(existing.ResponseBodyJson!) ?? "Bad request"),
                            existing.ResponseCode.Value)
                    };
                }

                using (_logger.BeginScope(new Dictionary<string, object?>
                       {
                           ["Outcome"] = "processing"
                       }))
                {
                    _logger.LogInformation("idempotency.processing");
                }
                return new IdempotencyResult<object>(
                    Result<object>.Conflict("Request is already processing"),
                    409);
            }

            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["Outcome"] = "started"
                   }))
            {
                _logger.LogInformation("idempotency.started");
            }

            var result = await handlerFunc(cancellationToken);

            var statusCode = result.Status switch
            {
                ResultStatus.Success => successStatusCode,
                ResultStatus.NotFound => 404,
                ResultStatus.Conflict => 409,
                _ => 400
            };

            string? responseJson = null;
            if (!result.IsSuccess && !string.IsNullOrWhiteSpace(result.Error))
            {
                responseJson = JsonSerializer.Serialize(result.Error);
            }

            var status = result.IsSuccess ? "completed" : "failed";
            await _store.CompleteAsync(key, scope, status, statusCode, responseJson, cancellationToken);

            using (_logger.BeginScope(new Dictionary<string, object?>
                   {
                       ["Outcome"] = status
                   }))
            {
                _logger.LogInformation("idempotency.completed");
            }

            return new IdempotencyResult<object>(new Result<object>(result.Status, default, result.Error), statusCode);
        }
    }
}
