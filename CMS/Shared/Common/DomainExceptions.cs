using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CMS.Shared.Common;

// Thrown by services, mapped to HTTP status codes by DomainExceptionFilter.
public sealed class ValidationFailedException(Dictionary<string, string[]> errors) : Exception("Validation failed")
{
    public Dictionary<string, string[]> Errors { get; } = errors;
    public static ValidationFailedException For(string field, string message) => new(new() { [field] = [message] });
}

public sealed class ForbiddenAccessException(string message) : Exception(message);
public sealed class ConflictException(string message) : Exception(message);

public sealed class DomainExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext ctx)
    {
        ctx.Result = ctx.Exception switch
        {
            // Keys are already camelCase field names, which the SPA maps straight onto its inputs.
            ValidationFailedException v => new BadRequestObjectResult(new ValidationProblemDetails(v.Errors)),
            ForbiddenAccessException f => new ObjectResult(new ProblemDetails { Status = 403, Detail = f.Message }) { StatusCode = 403 },
            ConflictException c => new ConflictObjectResult(new ProblemDetails { Status = 409, Detail = c.Message }),
            _ => null
        };
        ctx.ExceptionHandled = ctx.Result is not null;
    }
}
