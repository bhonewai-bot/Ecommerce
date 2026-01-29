namespace Ecommerce.Application.Common;

public enum ResultStatus
{
    Success,
    NotFound,
    BadRequest,
    Conflict
}

public record Result(ResultStatus Status, string? Error = null)
{
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Ok() => new(ResultStatus.Success);
    public static Result NotFound() => new(ResultStatus.NotFound);
    public static Result BadRequest(string message) => new(ResultStatus.BadRequest, message);
    public static Result Conflict(string message) => new(ResultStatus.Conflict, message);
}

public sealed record Result<T>(ResultStatus Status, T? Data = default, string? Error = null)
{
    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result<T> Ok(T data) => new(ResultStatus.Success, data);
    public static Result<T> NotFound() => new(ResultStatus.NotFound);
    public static Result<T> BadRequest(string message) => new(ResultStatus.BadRequest, default, message);
    public static Result<T> Conflict(string message) => new(ResultStatus.Conflict, default, message);
}
