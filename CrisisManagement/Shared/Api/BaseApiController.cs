using CrisisManagement.Shared.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CrisisManagement.Shared.Api;

// Base for the API controllers (same idea as SafetyNet BaseController). Every action wraps its call in
// try/catch and hands any exception to Failure(), which logs it and answers with the right status:
//   ValidationFailedException  -> 400 with the field errors (the SPA shows them next to the inputs)
//   ForbiddenAccessException   -> 403
//   ConflictException          -> 409
//   anything else              -> 500, with the message hidden outside Development
// The antiforgery token is required on every POST/PUT/DELETE globally (see Program.cs).
[ApiController]
[Route("api/[controller]")]
[Authorize]
public abstract class BaseApiController(ILogger logger) : ControllerBase
{
    protected IActionResult Failure(Exception ex, string action)
    {
        switch (ex)
        {
            case ValidationFailedException validation:
                // Expected: the caller broke a rule. Warning, no stack trace.
                logger.LogWarning("{Action} rejected: {Fields}", action, string.Join(", ", validation.Errors.Keys));
                return BadRequest(new ValidationProblemDetails(validation.Errors));

            case ForbiddenAccessException forbidden:
                logger.LogWarning("{Action} forbidden for {User}: {Message}", action, User.Identity?.Name, forbidden.Message);
                return Problem(detail: forbidden.Message, statusCode: StatusCodes.Status403Forbidden);

            case ConflictException conflict:
                logger.LogWarning("{Action} conflict: {Message}", action, conflict.Message);
                return Problem(detail: conflict.Message, statusCode: StatusCodes.Status409Conflict);

            case OperationCanceledException:
                // The browser went away; nothing to report.
                logger.LogDebug("{Action} cancelled by the client", action);
                return StatusCode(StatusCodes.Status499ClientClosedRequest);

            default:
                logger.LogError(ex, "{Action} failed", action);
                // An unexpected message can name tables, columns or paths, so outside Development the client
                // only gets the request id, the same one the log carries.
                var isDevelopment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment();
                var detail = isDevelopment ? ex.Message : $"An unexpected error occurred. Reference: {HttpContext.TraceIdentifier}";
                return Problem(detail: detail, statusCode: StatusCodes.Status500InternalServerError);
        }
    }
}
