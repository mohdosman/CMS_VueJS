using CrisisManagement.Data.Context;
using CrisisManagement.Data.Repositories.Interfaces;
using CrisisManagement.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Data.Repositories;

public sealed class RoleClaimRepository(AppDbContext context) : Repository<IdentityRoleClaim<int>>(context), IRoleClaimRepository
{
    public Task<List<string>> GetPermissionValuesAsync(int roleId) =>
        _entities.AsNoTracking()
            .Where(c => c.RoleId == roleId && c.ClaimType == AppClaimTypes.Permission && c.ClaimValue != null && c.ClaimValue != "")
            .Select(c => c.ClaimValue!).Distinct().OrderBy(v => v).ToListAsync();

    public Task<List<IdentityRoleClaim<int>>> GetPermissionClaimsAsync(int? roleId = null, string? value = null)
    {
        var q = _entities.Where(c => c.ClaimType == AppClaimTypes.Permission);
        if (roleId is int id) q = q.Where(c => c.RoleId == id);
        if (value is not null) q = q.Where(c => c.ClaimValue == value);
        return q.ToListAsync();
    }
}
