using System.Linq.Expressions;
using CrisisManagement.Data.Context;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Data.Repositories;
using CrisisManagement.Features.Users.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CrisisManagement.Features.Users.Repositories;

public sealed class UserRepository(AppDbContext context) : Repository<ApplicationUser>(context), IUserRepository
{
    private readonly AppDbContext _db = context;

    public async Task<PagedResult<UserListItem>> SearchAsync(UserSearchRequest req, UserSearchScope scope)
    {
        var q = _entities.AsNoTracking();

        if (!scope.IsAdmin)
        {
            var providerIds = scope.ProviderIds;
            var roleScope = scope.RoleIds;
            if (providerIds.Count == 0 || roleScope.Count == 0) return new();

            q = q.Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && roleScope.Contains(ur.RoleId)));
            q = q.Where(u => _db.ProviderUsers.Any(pu => pu.UserId == u.Id && providerIds.Contains(pu.ProviderId)));
        }

        if (Has(req.UserName)) { var v = $"%{req.UserName!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.UserName!, v)); }
        if (Has(req.FirstName)) { var v = $"%{req.FirstName!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.FirstName, v)); }
        if (Has(req.LastName)) { var v = $"%{req.LastName!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.LastName, v)); }
        if (Has(req.Email)) { var v = $"%{req.Email!.Trim()}%"; q = q.Where(u => EF.Functions.Like(u.Email!, v)); }

        if (ToBool(req.IsEnabled) is bool enabled) q = q.Where(u => u.IsActive == enabled);
        if (ToBool(req.IsADAccount) is bool ad) q = q.Where(u => u.IsADAccount == ad);

        var now = DateTimeOffset.UtcNow;
        if (ToBool(req.IsLockedOut) is bool locked)
            q = locked
                ? q.Where(u => u.LockoutEnabled && u.LockoutEnd >= now)
                : q.Where(u => !u.LockoutEnabled || u.LockoutEnd == null || u.LockoutEnd < now);

        if (req.RoleIds.Count > 0)
        {
            // A non-admin can only filter by roles inside their own scope.
            var roleIds = scope.IsAdmin ? req.RoleIds : req.RoleIds.Where(scope.RoleIds.Contains).ToList();
            if (roleIds.Count == 0) return new();
            q = q.Where(u => _db.UserRoles.Any(ur => ur.UserId == u.Id && roleIds.Contains(ur.RoleId)));
        }
        if (req.ProviderIds.Count > 0)
        {
            var providerIds = req.ProviderIds;
            q = q.Where(u => _db.ProviderUsers.Any(pu => pu.UserId == u.Id && providerIds.Contains(pu.ProviderId)));
        }

        q = (req.SortBy ?? "").ToLowerInvariant() switch
        {
            "firstname" => Order(q, u => u.FirstName, req.SortDesc),
            "lastname" => Order(q, u => u.LastName, req.SortDesc),
            "email" => Order(q, u => u.Email!, req.SortDesc),
            "isadaccount" => Order(q, u => u.IsADAccount, req.SortDesc),
            "isenabled" => Order(q, u => u.IsActive, req.SortDesc),
            "islockedout" => Order(q, u => u.LockoutEnabled && u.LockoutEnd >= now, req.SortDesc),
            _ => Order(q, u => u.UserName!, req.SortDesc)
        };

        var size = Math.Clamp(req.PageSize, 1, 200);
        var page = Math.Max(1, req.PageIndex);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * size).Take(size).Select(u => new UserListItem
        {
            UserKey = u.UserKey,
            UserName = u.UserName!,
            FirstName = u.FirstName,
            LastName = u.LastName,
            Email = u.Email ?? "",
            IsEnabled = u.IsActive,
            IsADAccount = u.IsADAccount,
            IsLockedOut = u.LockoutEnabled && u.LockoutEnd >= now
        }).ToListAsync();

        return new() { Items = items, TotalCount = total };
    }

    public Task<ApplicationUser?> GetByKeyAsync(Guid userKey) =>
        _entities.FirstOrDefaultAsync(x => x.UserKey == userKey);

    public Task<ApplicationUser?> GetByKeyNoTrackingAsync(Guid userKey) =>
        _entities.AsNoTracking().FirstOrDefaultAsync(x => x.UserKey == userKey);

    public Task<bool> UserNameExistsAsync(string normalizedUserName) =>
        _entities.AnyAsync(x => x.NormalizedUserName == normalizedUserName);

    private static bool Has(string? s) => !string.IsNullOrWhiteSpace(s);
    private static bool? ToBool(YesNoFilter f) => f switch { YesNoFilter.Yes => true, YesNoFilter.No => false, _ => null };
    private static IQueryable<ApplicationUser> Order<T>(IQueryable<ApplicationUser> q, Expression<Func<ApplicationUser, T>> key, bool desc) =>
        desc ? q.OrderByDescending(key) : q.OrderBy(key);
}
