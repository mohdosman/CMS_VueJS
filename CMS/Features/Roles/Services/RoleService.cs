using CMS.Data;
using CMS.Data.Models.Identity;
using CMS.Features.Menus;
using CMS.Features.Roles.ViewModels;
using CMS.Features.Users.ViewModels;
using CMS.Infrastructure.Authorization;
using CMS.Shared.Common;
using CMS.Shared.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace CMS.Features.Roles.Services;

// Port of the Blazor CMS RoleService + RolePermissionService. A role is saved together with its permission
// set in one transaction (the Blazor screen made two calls). Data access goes through the unit of work;
// RoleManager is used only for the Identity operations. Permission changes reach a signed-in user at their
// next sign-in, because permissions are copied onto the cookie principal then.
public sealed class RoleService(IUnitOfWork uow, RoleManager<ApplicationRole> roleManager, IMemoryCache cache, IHttpContextAccessor http, ILogger<RoleService> log)
{
    private static readonly string AdminMessage = $"The {AppRoles.Admin} role is built in and cannot be changed.";

    // ---------------------------------------------------------------- read

    public async Task<PagedResult<RoleListItem>> SearchAsync(RoleSearchRequest req, CancellationToken ct)
    {
        var size = Math.Clamp(req.PageSize, 1, 200);
        var (items, total) = await uow.Roles.SearchAsync(req.Name, req.SortBy, req.SortDesc, Math.Max(1, req.PageIndex), size);
        return new() { Items = items.Select(r => new RoleListItem(r.Id, r.Name)).ToList(), TotalCount = total };
    }

    public async Task<RoleDetail?> GetAsync(int id, CancellationToken ct)
    {
        var role = await uow.Roles.GetByIdAsync(id);
        return role is null ? null : new RoleDetail
        {
            Id = role.Id, Name = role.Name!, RowVersion = Convert.ToBase64String(role.Version),
            Permissions = await uow.RoleClaims.GetPermissionValuesAsync(id)
        };
    }

    // Groups in the order the menu is laid out, so related permissions sit together.
    public async Task<List<PermissionGroupItem>> GetPermissionGroupsAsync(CancellationToken ct)
    {
        var order = MenuOrder(await uow.MenuItems.GetAllAsync());
        int Rank(Permission p) => order.GetValueOrDefault(p.MenuItemId, int.MaxValue - 1);

        return (await uow.PermissionGroups.GetAllWithPermissionsAsync())
            .Select(g => (g.GroupName, Items: g.Permissions.Where(p => p.Value != "").OrderBy(Rank).ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase).ToList()))
            .Where(g => g.Items.Count > 0)
            .OrderBy(g => g.Items.Min(Rank)).ThenBy(g => g.GroupName, StringComparer.OrdinalIgnoreCase)
            .Select(g => new PermissionGroupItem(g.GroupName, g.Items.Select(p => new PermissionItem(p.Name, p.Value, p.Description)).ToList()))
            .ToList();
    }

    private static Dictionary<int, int> MenuOrder(List<MenuItem> all)
    {
        var byParent = all.ToLookup(m => m.ParentMenuItemId);
        var map = new Dictionary<int, int>();
        void Visit(int? parent)
        {
            foreach (var m in byParent[parent].OrderBy(m => m.DisplaySequence).ThenBy(m => m.MenuItemName))
            {
                map[m.MenuItemId] = map.Count;
                Visit(m.MenuItemId);
            }
        }
        Visit(null);
        return map;
    }

    // ---------------------------------------------------------------- write

    public async Task<RoleDetail> CreateAsync(RoleEditRequest r, CancellationToken ct)
    {
        var name = await ValidateAsync(r, exceptId: 0);
        var role = new ApplicationRole(name);
        EnsureGrantable(Desired(r.Permissions));

        await using var tx = await uow.BeginTransactionAsync(ct);
        var created = await roleManager.CreateAsync(role);
        if (!created.Succeeded) throw ToValidation(created);

        await CreateScopePermissionAsync(name);
        SetClaims(role.Id, r.Permissions, []);
        await uow.SaveChangesAsync();
        await tx.CommitAsync(ct);
        Invalidate();

        return (await GetAsync(role.Id, ct))!;
    }

    // Null = role not found.
    public async Task<RoleDetail?> UpdateAsync(int id, RoleEditRequest r, CancellationToken ct)
    {
        var role = await uow.Roles.GetByIdAsync(id);
        if (role is null) return null;
        if (IsAdminRole(role.Name)) throw new ConflictException(AdminMessage);

        var name = await ValidateAsync(r, exceptId: id);

        // Compared here rather than via the tracked OriginalValue, which Identity validation queries overwrite.
        if (!string.IsNullOrWhiteSpace(r.RowVersion) && !role.Version.AsSpan().SequenceEqual(Convert.FromBase64String(r.RowVersion)))
            throw new ConflictException("This role was changed by someone else. Reload the page and try again.");

        var oldName = role.Name!;
        var have = await uow.RoleClaims.GetPermissionClaimsAsync(id);
        var desired = Desired(r.Permissions);
        EnsureGrantable(desired.Except(have.Select(c => c.ClaimValue!), StringComparer.OrdinalIgnoreCase)
            .Concat(have.Select(c => c.ClaimValue!).Except(desired, StringComparer.OrdinalIgnoreCase)));

        await using var tx = await uow.BeginTransactionAsync(ct);
        role.Name = name;
        var updated = await roleManager.UpdateAsync(role);
        if (!updated.Succeeded) throw ToValidation(updated);

        await RenameScopePermissionAsync(oldName, name);
        SetClaims(id, r.Permissions, have);
        await uow.SaveChangesAsync();
        await tx.CommitAsync(ct);
        Invalidate();

        return await GetAsync(id, ct);
    }

    // False = role not found.
    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var role = await uow.Roles.GetByIdAsync(id);
        if (role is null) return false;
        if (IsAdminRole(role.Name)) throw new ConflictException(AdminMessage);

        var holders = await uow.UserRoles.CountForRoleAsync(id);
        if (holders > 0)
            throw new ConflictException($"{holders} user(s) still hold this role. Remove it from them before deleting the role.");

        await using var tx = await uow.BeginTransactionAsync(ct);
        var scope = UserRolePermissionConstants.PermissionValue(role.Name!);
        uow.RoleClaims.RemoveRange(await uow.RoleClaims.GetPermissionClaimsAsync(id));
        // The scope permission goes with the role, and so does every grant of it to other roles.
        uow.RoleClaims.RemoveRange(await uow.RoleClaims.GetPermissionClaimsAsync(value: scope));
        if (await uow.Permissions.GetByValueAsync(scope) is { } perm) uow.Permissions.Remove(perm);
        await uow.SaveChangesAsync();

        var deleted = await roleManager.DeleteAsync(role);
        if (!deleted.Succeeded) throw ToValidation(deleted);
        await tx.CommitAsync(ct);
        Invalidate();
        return true;
    }

    // ---------------------------------------------------------------- helpers

    // Returns the trimmed name; throws field errors (camelCase keys) for anything wrong.
    private async Task<string> ValidateAsync(RoleEditRequest r, int exceptId)
    {
        var errors = new Dictionary<string, string[]>();
        var name = r.Name?.Trim() ?? "";
        if (name.Length == 0) errors["name"] = ["Role name is required."];
        else if (name.Length > RoleFieldLimits.MaxNameLength) errors["name"] = [$"Role name cannot exceed {RoleFieldLimits.MaxNameLength} characters."];
        else if (await uow.Roles.NameExistsAsync(roleManager.NormalizeKey(name)!, exceptId)) errors["name"] = ["A role with this name already exists."];

        var known = (await uow.Permissions.GetAllValuesAsync()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unknown = r.Permissions.Where(v => !string.IsNullOrWhiteSpace(v) && !known.Contains(v.Trim())).ToList();
        if (unknown.Count > 0) errors["permissions"] = [$"Unknown permission: {string.Join(", ", unknown)}."];

        if (errors.Count > 0) throw new ValidationFailedException(errors);
        return name;
    }

    // Diff against what the role holds: add the new values, drop the ones no longer wanted.
    private static HashSet<string> Desired(IEnumerable<string> wanted) =>
        wanted.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);

    // Escalation guard: a non-admin may only grant or remove permissions they hold themselves (x.edit
    // counts as x.view), otherwise roles.edit would let them give themselves anything. Administrators are unrestricted.
    private void EnsureGrantable(IEnumerable<string> changed)
    {
        var user = http.HttpContext!.User;
        if (user.IsInRole(AppRoles.Admin)) return;

        var held = user.FindAll(AppClaimTypes.Permission).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var v in held.Where(v => v.EndsWith(".edit", StringComparison.OrdinalIgnoreCase)).ToList()) held.Add(v[..^5] + ".view");

        var denied = changed.Where(v => !held.Contains(v)).ToList();
        if (denied.Count > 0)
            throw new ForbiddenAccessException($"You can only grant or remove permissions you hold yourself: {string.Join(", ", denied)}.");
    }

    private void SetClaims(int roleId, IEnumerable<string> wanted, List<IdentityRoleClaim<int>> existing)
    {
        var desired = Desired(wanted);
        var have = existing.Select(c => c.ClaimValue!).ToHashSet(StringComparer.OrdinalIgnoreCase);
        uow.RoleClaims.AddRange(desired.Except(have).Select(v => new IdentityRoleClaim<int>
            { RoleId = roleId, ClaimType = AppClaimTypes.Permission, ClaimValue = v }));
        uow.RoleClaims.RemoveRange(existing.Where(c => !desired.Contains(c.ClaimValue!)));
    }

    // "users.role.<slug>" lets non-admins be granted visibility of this role on the user screens. Skipped,
    // with a warning, when the Users menu item is missing (same as the Blazor CMS).
    private async Task CreateScopePermissionAsync(string roleName)
    {
        var value = UserRolePermissionConstants.PermissionValue(roleName);
        if (await uow.Permissions.GetByValueAsync(value) is not null) return;

        var menu = await uow.MenuItems.GetByNameAsync(UserRolePermissionConstants.MenuItemName);
        if (menu is null)
        {
            log.LogWarning("Menu item {Menu} not found. Scope permission not created for role {Role}", UserRolePermissionConstants.MenuItemName, roleName);
            return;
        }

        var group = await uow.PermissionGroups.GetByNameAsync(UserRolePermissionConstants.PermissionGroupName);
        if (group is null)
        {
            group = new PermissionGroup { GroupName = UserRolePermissionConstants.PermissionGroupName, Description = "Permissions for the Users page" };
            uow.PermissionGroups.Add(group);
        }

        uow.Permissions.Add(new Permission
        {
            PermissionGroupName = group, MenuItemId = menu.MenuItemId,
            Name = $"UserRole:{roleName}",
            Description = $"Permission to see and assign the [{roleName}] role on user screens",
            Value = value
        });
    }

    private async Task RenameScopePermissionAsync(string oldName, string newName)
    {
        var oldValue = UserRolePermissionConstants.PermissionValue(oldName);
        var newValue = UserRolePermissionConstants.PermissionValue(newName);

        var perm = await uow.Permissions.GetByValueAsync(oldValue);
        if (perm is null)
        {
            await CreateScopePermissionAsync(newName);   // a role that predates this feature
            return;
        }

        perm.Name = $"UserRole:{newName}";
        perm.Description = $"Permission to see and assign the [{newName}] role on user screens";
        perm.Value = newValue;
        if (!string.Equals(oldValue, newValue, StringComparison.OrdinalIgnoreCase))
            foreach (var c in await uow.RoleClaims.GetPermissionClaimsAsync(value: oldValue)) c.ClaimValue = newValue;
    }

    private void Invalidate()
    {
        cache.Remove(PermissionCatalog.CacheKey);
        cache.Remove(MenuService.CacheKey);
    }

    private static bool IsAdminRole(string? name) => string.Equals(name, AppRoles.Admin, StringComparison.OrdinalIgnoreCase);

    private static Exception ToValidation(IdentityResult result)
    {
        if (result.Errors.Any(e => e.Code == "ConcurrencyFailure"))
            return new ConflictException("This role was changed by someone else. Reload the page and try again.");
        return ValidationFailedException.For("name", string.Join(" ", result.Errors.Select(e => e.Description)));
    }
}
