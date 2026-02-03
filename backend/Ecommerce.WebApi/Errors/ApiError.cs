using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Ecommerce.WebApi.Errors;

public sealed record ApiError(string Code, string Message);

public static class ApiErrorCodes
{
    public const string BadRequest = "BAD_REQUEST";
    public const string NotFound = "NOT_FOUND";
    public const string Conflict = "CONFLICT";
    public const string ValidationFailed = "VALIDATION_FAILED";
    public const string Unexpected = "UNEXPECTED_ERROR";
    public const string IdempotencyKeyRequired = "IDEMPOTENCY_KEY_REQUIRED";
}

public static class ApiErrorFactory
{
    public static ApiError BadRequest(string? message = null, string? code = null)
        => new(code ?? ApiErrorCodes.BadRequest, NormalizeMessage(message, "Bad request."));

    public static ApiError Conflict(string? message = null, string? code = null)
        => new(code ?? ApiErrorCodes.Conflict, NormalizeMessage(message, "Conflict."));

    public static ApiError NotFound(string? message = null, string? code = null)
        => new(code ?? ApiErrorCodes.NotFound, NormalizeMessage(message, "Resource not found."));

    public static ApiError ValidationFailed(ModelStateDictionary modelState)
    {
        var message = modelState.Values
            .SelectMany(entry => entry.Errors)
            .Select(error => error.ErrorMessage)
            .FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

        return new ApiError(ApiErrorCodes.ValidationFailed, NormalizeMessage(message, "Validation failed."));
    }

    public static ApiError Unexpected()
        => new(ApiErrorCodes.Unexpected, "An unexpected error occurred.");

    private static string NormalizeMessage(string? message, string defaultMessage)
        => string.IsNullOrWhiteSpace(message) ? defaultMessage : message;
}

public static class ControllerBaseErrorExtensions
{
    public static ActionResult ApiBadRequest(this ControllerBase controller, string? message = null, string? code = null)
        => controller.BadRequest(ApiErrorFactory.BadRequest(message, code));

    public static ActionResult ApiConflict(this ControllerBase controller, string? message = null, string? code = null)
        => controller.Conflict(ApiErrorFactory.Conflict(message, code));

    public static ActionResult ApiNotFound(this ControllerBase controller, string? message = null, string? code = null)
        => controller.NotFound(ApiErrorFactory.NotFound(message, code));
}
