namespace CMS.Shared.Common;

// Thrown by services, mapped to HTTP status codes by BaseApiController.Failure.
public sealed class ValidationFailedException(Dictionary<string, string[]> errors) : Exception("Validation failed")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
    public static ValidationFailedException For(string field, string message) => new(new() { [field] = [message] });
}

public sealed class ForbiddenAccessException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);
