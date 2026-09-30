using System.Security.Claims;
using CrisisManagement.Data;
using CrisisManagement.Data.Models.Domain;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace CrisisManagement.Infrastructure.Identity;

// Identity already adds name, roles and the role "permission" claims; this adds the
// provider scope the Blazor CMS carried in its JWT. Omitted when the user has none.
public sealed class CmsClaimsPrincipalFactory(
    UserManager<ApplicationUser> users, RoleManager<ApplicationRole> roles,
    IOptions<IdentityOptions> options, IUnitOfWork uow, MfaService mfa)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(users, roles, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        var providerIds = await uow.ProviderUsers.GetProviderIdsForUserAsync(user.Id);
        if (providerIds.Count > 0)
            identity.AddClaim(new Claim(AppClaimTypes.ProviderIds, string.Join(",", providerIds)));

        // The two things a user can owe before working. They ride in the cookie and are refreshed when settled.
        if (user.IsTemporaryPassword)
            identity.AddClaim(new Claim(AppClaimTypes.IsTemporaryPassword, bool.TrueString));
        if (mfa.IsRequired(user) && !await mfa.IsEnrolledAsync(user))
            identity.AddClaim(new Claim(AppClaimTypes.RequiresMfaSetup, bool.TrueString));

        return identity;
    }
}
