using CMS.Data.Models.Identity;
using CMS.Data.Repositories.Interfaces;
using CMS.Features.Users.ViewModels;

namespace CMS.Features.Users.Repositories;

// What the caller may see. Admins are unrestricted; everyone else is limited to users holding one of
// RoleIds and sharing one of ProviderIds (built by the service from the caller claims).
public sealed record UserSearchScope(bool IsAdmin, IReadOnlySet<int> RoleIds, IReadOnlyCollection<int> ProviderIds);

public interface IUserRepository : IRepository<ApplicationUser>
{
    Task<PagedResult<UserListItem>> SearchAsync(UserSearchRequest request, UserSearchScope scope);

    // Tracked (for updates) and untracked (for reads).
    Task<ApplicationUser?> GetByKeyAsync(Guid userKey);
    Task<ApplicationUser?> GetByKeyNoTrackingAsync(Guid userKey);

    Task<bool> UserNameExistsAsync(string normalizedUserName);
}
