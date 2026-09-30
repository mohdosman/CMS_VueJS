using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;
using CMS.Features.Users.ViewModels;

namespace CMS.Features.Users.Mapping;

public static class UserMappings
{
    // lastLogon is the newest RBS_Logon row; the user own LastLoginDate (set by this app) counts too.
    public static UserDetail ToDetail(this ApplicationUser u, IReadOnlyList<IdName> roles, IReadOnlyList<IdName> providers, DateTime? lastLogon)
    {
        DateTime? own = u.LastLoginDate > new DateTime(1900, 1, 1) ? u.LastLoginDate : null;

        return new UserDetail
        {
            UserKey = u.UserKey,
            RowVersion = Convert.ToBase64String(u.Version),
            UserName = u.UserName!,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email ?? "",
            PhoneNumber = u.PhoneNumber,
            Notes = u.Comment ?? "",
            IsEnabled = u.IsActive,
            IsADAccount = u.IsADAccount,
            IsLockedOut = u.LockoutEnabled && u.LockoutEnd >= DateTimeOffset.UtcNow,
            TwoFactorEnabled = u.TwoFactorEnabled,
            LastLoginAt = lastLogon > own ? lastLogon : own,
            LastPasswordChangedDate = u.LastPasswordChangedDate,
            CreatedOn = u.CreatedOn,
            UpdatedOn = u.UpdatedOn,
            Roles = roles.Select(r => r.Name).ToList(),
            RoleIds = roles.Select(r => r.Id).ToList(),
            Providers = providers.Select(p => p.Name).ToList(),
            ProviderIds = providers.Select(p => p.Id).ToList()
        };
    }
}
