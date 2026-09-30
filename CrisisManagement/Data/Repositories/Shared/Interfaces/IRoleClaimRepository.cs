using CrisisManagement.Data.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace CrisisManagement.Data.Repositories.Interfaces;

// RBS_RoleClaim rows; permissions are the ones whose type is "permission".
public interface IRoleClaimRepository : IRepository<IdentityRoleClaim<int>>
{
    Task<List<string>> GetPermissionValuesAsync(int roleId);
    // Tracked, for change. roleId null = every role (used when a permission value is renamed or removed).
    Task<List<IdentityRoleClaim<int>>> GetPermissionClaimsAsync(int? roleId = null, string? value = null);
}
