using System.Security.Claims;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Constants;

namespace CrisisManagement.Infrastructure.Identity;

// Which providers the signed-in user may work with: administrators all of them, everyone else only the ids in their
// provider_ids claim (none = nothing). One place for the rule every provider-scoped screen applies on each read and write.
public sealed class ProviderScope(IHttpContextAccessor http)
{
    private ClaimsPrincipal User => http.HttpContext!.User;

    public bool IsAdmin => User.IsInRole(AppRoles.Admin);

    // null = unrestricted (administrator); otherwise the allowed provider ids, possibly none.
    public IReadOnlyList<int>? AllowedIds => IsAdmin ? null :
        (User.FindFirstValue(AppClaimTypes.ProviderIds) ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(v => int.TryParse(v, out var id) ? id : 0).Where(id => id > 0).ToArray();

    public bool Allows(int providerId) => AllowedIds is not { } ids || ids.Contains(providerId);

    // Throws Forbidden when the caller may not use the provider.
    public void Require(int providerId, string message = "You do not have access to this provider.")
    {
        if (!Allows(providerId)) throw new ForbiddenAccessException(message);
    }
}
