using CMS.Data;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace CMS.Infrastructure.Authorization;

// Same model as the Blazor CMS: any policy name containing a dot is a permission
// ("users.view"), built on demand - no AddPolicy calls. Use [Authorize(Policy = "users.view")].
public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();
    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName) =>
        policyName.Contains('.')
            ? Task.FromResult<AuthorizationPolicy?>(new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new PermissionRequirement(policyName))
                .Build())
            : _fallback.GetPolicyAsync(policyName);
}

// Known permission values from RBS_Permission, cached 2 minutes. A permission that is not
// in the table never grants access, so a typo'd policy name fails closed.
public sealed class PermissionCatalog(IUnitOfWork uow, IMemoryCache cache)
{
    public async Task<bool> IsKnownAsync(string permission)
    {
        var all = await cache.GetOrCreateAsync("perm:catalog", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2);
            var values = await uow.Permissions.GetAllValuesAsync();
            return values.ToHashSet(StringComparer.OrdinalIgnoreCase);
        });
        return all!.Contains(permission);
    }
}

public sealed class PermissionHandler(PermissionCatalog catalog) : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var user = context.User;
        if (user.Identity?.IsAuthenticated != true) return;

        if (user.IsInRole(AppRoles.Admin)) { context.Succeed(requirement); return; }

        var required = requirement.Permission;
        if (!await catalog.IsKnownAsync(required)) return;

        // x.edit implies x.view.
        var editEquivalent = required.EndsWith(".view", StringComparison.OrdinalIgnoreCase) ? required[..^5] + ".edit" : null;
        if (user.HasClaim(AppClaimTypes.Permission, required) ||
            (editEquivalent is not null && user.HasClaim(AppClaimTypes.Permission, editEquivalent)))
            context.Succeed(requirement);
    }
}
