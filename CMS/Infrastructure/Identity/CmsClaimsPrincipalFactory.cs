using System.Security.Claims;
using CMS.Data.Context;
using CMS.Data.Models;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace CMS.Infrastructure.Identity;

// Identity already adds name, roles and the role "permission" claims; this adds the
// provider scope the Blazor CMS carried in its JWT. Omitted when the user has none.
public sealed class CmsClaimsPrincipalFactory(
    UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles,
    IOptions<IdentityOptions> options, AppDbContext db)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(users, roles, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var providerIds = await db.ProviderUsers.AsNoTracking()
            .Where(pu => pu.UserId == user.Id).Select(pu => pu.ProviderId).Distinct().ToListAsync();
        if (providerIds.Count > 0)
            identity.AddClaim(new Claim(AppClaimTypes.ProviderIds, string.Join(",", providerIds)));

        return identity;
    }
}
