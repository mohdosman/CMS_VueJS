using System.Security.Claims;

namespace CMS.Shared.Extensions;

// Ported from SafetyNet (Shared/Extensions/ClaimsPrincipalExtensions.cs).
public static class ClaimsPrincipalExtensions
{
    public static int GetUserId(this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        var claim = principal.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }
}
