using Ecommerce.Application.Common;

namespace Ecommerce.Application.Features.Idempotency;

public sealed record IdempotencyResult<T>(Result<T> Result, int StatusCode);
