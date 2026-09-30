using CMS.Shared.Constants;

namespace CMS.Infrastructure.Identity;

// Blocks the application for a signed-in user who owes the account an action before they can work: changing a
// temporary password, or enrolling an authenticator when MFA is required (port of the Blazor CMS middleware).
// Both gates are default-deny: a request is refused unless its path is on the gate's allowlist, so pages and
// endpoints added later are covered without touching this file. The password gate wins when both apply, because
// someone who owes both should fix the password before enrolling an authenticator against it.
// The claims are written at sign-in (CmsClaimsPrincipalFactory) and refreshed when the action is completed.
public sealed class RequiredAccountActionMiddleware(RequestDelegate next)
{
    private static readonly string[] PasswordGate = ["/Account/Change", "/Account/Logout"];
    private static readonly string[] MfaGate = ["/Account/SetupMfa", "/Account/BeginMfaSetup", "/Account/Logout"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            if (Has(context, AppClaimTypes.IsTemporaryPassword))
            {
                if (!IsAllowed(context.Request, PasswordGate))
                {
                    await RefuseAsync(context, "Your temporary password must be changed before you continue.", "/Account/Change");
                    return;
                }
            }
            else if (Has(context, AppClaimTypes.RequiresMfaSetup) && !IsAllowed(context.Request, MfaGate))
            {
                await RefuseAsync(context, "Two-factor authentication must be set up before you continue.", "/Account/SetupMfa");
                return;
            }
        }

        await next(context);
    }

    private static bool Has(HttpContext context, string claimType) =>
        string.Equals(context.User.FindFirst(claimType)?.Value, bool.TrueString, StringComparison.OrdinalIgnoreCase);

    private static bool IsAllowed(HttpRequest request, string[] allowed) =>
        allowed.Any(p => request.Path.Equals(p, StringComparison.OrdinalIgnoreCase));

    // API callers get JSON they can act on; browser pages are sent straight to the remedy.
    private static async Task RefuseAsync(HttpContext context, string message, string redirectTo)
    {
        if (context.Request.Path.StartsWithSegments("/api", StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { message, redirectTo });
            return;
        }
        context.Response.Redirect(redirectTo);
    }
}
