using CrisisManagement.Data;
using CrisisManagement.Data.Models.Identity;
using CrisisManagement.Features.Menus.ViewModels;
using CrisisManagement.Infrastructure.Authorization;
using CrisisManagement.Shared.Common;
using CrisisManagement.Shared.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace CrisisManagement.Features.Menus;

// Port of the Blazor CMS menu / permission / permission-group administration. Differences on purpose: real menus.*
// permissions, a duplicate-name and a parent-cycle check, no delete while a menu still has sub-menus, and role claims
// follow a permission when it is renamed or deleted (Blazor left them orphaned). Every change clears the menu and
// permission caches so the navbar and the policy catalog see it at once. Data access goes through the unit of work.
public sealed class MenuAdminService(IUnitOfWork uow, IMemoryCache cache)
{
    private void Invalidate()
    {
        cache.Remove(PermissionCatalog.CacheKey);
        cache.Remove(MenuService.CacheKey);
    }

    // ---------------------------------------------------------------- menus

    public async Task<List<MenuListItem>> ListAsync() =>
        (await uow.MenuItems.GetAllAsync()).OrderBy(m => m.DisplaySequence).ThenBy(m => m.MenuItemName)
            .Select(m => new MenuListItem(m.MenuItemId, m.MenuItemName, m.Icon, m.Url, m.ParentMenuItemId, m.DisplaySequence, m.IsAlwaysEnabled, m.IsEnabled)).ToList();

    public async Task<MenuDetail?> GetAsync(int id) =>
        (await uow.MenuItems.GetAllAsync()).FirstOrDefault(m => m.MenuItemId == id) is { } m ? ToDetail(m) : null;

    // Every item that may become the parent of excludeId: not itself and not one of its own descendants.
    public async Task<List<ParentOption>> ParentsAsync(int? excludeId)
    {
        var all = await uow.MenuItems.GetAllAsync();
        var excluded = excludeId is int id ? Descendants(all, id).Append(id).ToHashSet() : [];
        var byId = all.ToDictionary(m => m.MenuItemId);

        string PathOf(MenuItem m)
        {
            var names = new List<string>();
            for (var cur = m; cur is not null; cur = cur.ParentMenuItemId is int p && byId.TryGetValue(p, out var up) ? up : null)
                names.Insert(0, cur.MenuItemName);
            return string.Join(" > ", names);
        }

        return all.Where(m => !excluded.Contains(m.MenuItemId)).Select(m => new ParentOption(m.MenuItemId, PathOf(m)))
            .OrderBy(o => o.Path, StringComparer.OrdinalIgnoreCase).ToList();
    }

    public async Task<MenuDetail> CreateAsync(MenuEditRequest r)
    {
        var errors = new ErrorBag();
        var name = await ValidateAsync(r, 0, errors);
        await CheckParentAsync(r.ParentId, null, errors);
        errors.ThrowIfAny();

        var m = new MenuItem { MenuItemName = name };
        Apply(m, r);
        uow.MenuItems.Add(m);
        await uow.SaveChangesAsync();
        Invalidate();
        return (await GetAsync(m.MenuItemId))!;
    }

    // Null = menu not found.
    public async Task<MenuDetail?> UpdateAsync(int id, MenuEditRequest r)
    {
        var m = await uow.MenuItems.GetTrackedAsync(id);
        if (m is null) return null;

        var errors = new ErrorBag();
        var name = await ValidateAsync(r, id, errors);
        await CheckParentAsync(r.ParentId, id, errors);
        errors.ThrowIfAny();

        // Compared here rather than via the tracked OriginalValue, which reloads overwrite.
        if (!string.IsNullOrWhiteSpace(r.RowVersion) && !m.Version.AsSpan().SequenceEqual(Convert.FromBase64String(r.RowVersion)))
            throw new ConflictException("This menu item was changed by someone else. Reload the page and try again.");

        m.MenuItemName = name;
        Apply(m, r);
        await uow.SaveChangesAsync();
        Invalidate();
        return await GetAsync(id);
    }

    // False = menu not found. A menu with sub-menus is refused (they would vanish from the navbar). Its permissions go
    // with it, together with any role claims that granted them.
    public async Task<bool> DeleteAsync(int id)
    {
        var m = await uow.MenuItems.GetTrackedAsync(id);
        if (m is null) return false;

        if ((await uow.MenuItems.GetAllAsync()).Any(x => x.ParentMenuItemId == id))
            throw new ConflictException("This menu item still has sub-menus. Move or delete them first.");

        await using var tx = await uow.BeginTransactionAsync();
        foreach (var p in m.Permissions)
            uow.RoleClaims.RemoveRange(await uow.RoleClaims.GetPermissionClaimsAsync(value: p.Value));
        uow.Permissions.RemoveRange(m.Permissions);
        uow.MenuItems.Remove(m);
        await uow.SaveChangesAsync();
        await tx.CommitAsync();
        Invalidate();
        return true;
    }

    private async Task<string> ValidateAsync(MenuEditRequest r, int exceptId, ErrorBag e)
    {
        var name = e.Required("name", "Menu item name", r.Name, MenuFieldLimits.MaxNameLength);
        if (name.Length > 0 && !e.Has("name") && await uow.MenuItems.NameExistsAsync(name, exceptId))
            e.Add("name", "A menu item with this name already exists.");

        e.Optional("icon", "Icon", r.Icon, MenuFieldLimits.MaxIconLength);
        e.Optional("description", "Description", r.Description, MenuFieldLimits.MaxTextLength);
        e.Optional("comment", "Comment", r.Comment, MenuFieldLimits.MaxTextLength);
        e.Optional("url", "Url", r.Url, MenuFieldLimits.MaxUrlLength);
        e.Optional("detailUrl", "Detail url", r.DetailUrl, MenuFieldLimits.MaxUrlLength);
        e.Optional("templateUrl", "Template url", r.TemplateUrl, MenuFieldLimits.MaxUrlLength);
        e.Optional("detailTemplateUrl", "Detail template url", r.DetailTemplateUrl, MenuFieldLimits.MaxUrlLength);
        e.Optional("apiUrl", "Api url", r.ApiUrl, MenuFieldLimits.MaxUrlLength);
        if (r.DisplaySequence is < 0 or > 255) e.Add("displaySequence", "Display order must be between 0 and 255.");
        return name;
    }

    // The parent must exist, and (on update) must not be the item itself or one of its descendants.
    private async Task CheckParentAsync(int? parentId, int? itemId, ErrorBag e)
    {
        if (parentId is not int pid) return;
        var all = await uow.MenuItems.GetAllAsync();
        if (all.All(m => m.MenuItemId != pid)) e.Add("parentId", "The selected parent does not exist.");
        else if (itemId is int id)
        {
            if (pid == id) e.Add("parentId", "A menu item cannot be its own parent.");
            else if (Descendants(all, id).Contains(pid)) e.Add("parentId", "A menu item cannot be moved under one of its own sub-menus.");
        }
    }

    private static List<int> Descendants(List<MenuItem> all, int id)
    {
        var byParent = all.ToLookup(m => m.ParentMenuItemId);
        var found = new List<int>();
        void Walk(int parent) { foreach (var c in byParent[parent]) { found.Add(c.MenuItemId); Walk(c.MenuItemId); } }
        Walk(id);
        return found;
    }

    private static void Apply(MenuItem m, MenuEditRequest r)
    {
        m.Icon = r.Icon?.Trim() ?? "";
        m.Description = r.Description?.Trim() ?? "";
        m.Comment = r.Comment?.Trim() ?? "";
        m.Url = r.Url?.Trim() ?? "";
        m.DetailUrl = r.DetailUrl?.Trim() ?? "";
        m.TemplateUrl = r.TemplateUrl?.Trim() ?? "";
        m.DetailTemplateUrl = r.DetailTemplateUrl?.Trim() ?? "";
        m.ApiUrl = r.ApiUrl?.Trim() ?? "";
        m.ParentMenuItemId = r.ParentId;
        m.DisplaySequence = (byte)Math.Clamp(r.DisplaySequence, 0, 255);
        m.IsAlwaysEnabled = r.IsAlwaysEnabled;
        m.IsEnabled = r.IsEnabled;
    }

    private static MenuDetail ToDetail(MenuItem m) => new()
    {
        Id = m.MenuItemId, RowVersion = Convert.ToBase64String(m.Version), Name = m.MenuItemName, Icon = m.Icon, Description = m.Description,
        Url = m.Url, DetailUrl = m.DetailUrl, TemplateUrl = m.TemplateUrl, DetailTemplateUrl = m.DetailTemplateUrl, ApiUrl = m.ApiUrl,
        Comment = m.Comment, ParentId = m.ParentMenuItemId, DisplaySequence = m.DisplaySequence, IsAlwaysEnabled = m.IsAlwaysEnabled, IsEnabled = m.IsEnabled
    };

    // ---------------------------------------------------------------- permissions

    // Null = menu not found.
    public async Task<List<PermissionRow>?> PermissionsAsync(int menuId)
    {
        if ((await uow.MenuItems.GetAllAsync()).All(m => m.MenuItemId != menuId)) return null;
        return (await uow.Permissions.GetForMenuAsync(menuId))
            .Select(p => new PermissionRow(p.PermissionId, p.PermissionGroupNameId, p.PermissionGroupName.GroupName, p.Name, p.Value, p.Description)).ToList();
    }

    public async Task<PermissionRow> CreatePermissionAsync(PermissionEditRequest r)
    {
        if ((await uow.MenuItems.GetAllAsync()).All(m => m.MenuItemId != r.MenuId))
            throw ValidationFailedException.For("form", "The menu item does not exist.");

        var errors = new ErrorBag();
        var (name, value, description) = await ValidatePermissionAsync(r, 0, errors);
        errors.ThrowIfAny();

        var p = new Permission { MenuItemId = r.MenuId, PermissionGroupNameId = r.GroupId, Name = name, Value = value, Description = description };
        uow.Permissions.Add(p);
        await uow.SaveChangesAsync();
        Invalidate();
        return (await PermissionsAsync(r.MenuId))!.First(x => x.Id == p.PermissionId);
    }

    // Null = permission not found. Renaming the value moves the role claims that granted it.
    public async Task<PermissionRow?> UpdatePermissionAsync(int id, PermissionEditRequest r)
    {
        var p = await uow.Permissions.GetByIdAsync(id);
        if (p is null) return null;

        var errors = new ErrorBag();
        var (name, value, description) = await ValidatePermissionAsync(r, id, errors);
        errors.ThrowIfAny();

        await using var tx = await uow.BeginTransactionAsync();
        if (!string.Equals(p.Value, value, StringComparison.Ordinal))
            foreach (var c in await uow.RoleClaims.GetPermissionClaimsAsync(value: p.Value)) c.ClaimValue = value;
        p.PermissionGroupNameId = r.GroupId;
        p.Name = name; p.Value = value; p.Description = description;
        await uow.SaveChangesAsync();
        await tx.CommitAsync();
        Invalidate();
        return (await PermissionsAsync(p.MenuItemId))!.First(x => x.Id == id);
    }

    // False = permission not found. Role claims that granted it are removed with it.
    public async Task<bool> DeletePermissionAsync(int id)
    {
        var p = await uow.Permissions.GetByIdAsync(id);
        if (p is null) return false;

        await using var tx = await uow.BeginTransactionAsync();
        uow.RoleClaims.RemoveRange(await uow.RoleClaims.GetPermissionClaimsAsync(value: p.Value));
        uow.Permissions.Remove(p);
        await uow.SaveChangesAsync();
        await tx.CommitAsync();
        Invalidate();
        return true;
    }

    private async Task<(string Name, string Value, string Description)> ValidatePermissionAsync(PermissionEditRequest r, int exceptId, ErrorBag e)
    {
        var name = e.Required("name", "Name", r.Name, PermissionFieldLimits.MaxPermissionTextLength);
        var value = e.Required("value", "Value", r.Value, PermissionFieldLimits.MaxPermissionTextLength);
        var description = e.Required("description", "Description", r.Description, PermissionFieldLimits.MaxPermissionTextLength);
        if (value.Length > 0 && !e.Has("value") && value.Any(char.IsWhiteSpace)) e.Add("value", "Value cannot contain spaces.");
        // Claims are matched by value across the whole application, so a value can exist only once.
        if (value.Length > 0 && !e.Has("value") && await uow.Permissions.ValueExistsAsync(value, exceptId))
            e.Add("value", "A permission with this value already exists.");
        if ((await uow.PermissionGroups.GetRowsAsync()).All(g => g.Id != r.GroupId)) e.Add("groupId", "Choose a permission group.");
        return (name, value, description);
    }

    // ---------------------------------------------------------------- permission groups

    public Task<List<GroupRow>> GroupsAsync() => uow.PermissionGroups.GetRowsAsync();

    public async Task<GroupRow> CreateGroupAsync(GroupEditRequest r)
    {
        var errors = new ErrorBag();
        var name = await ValidateGroupAsync(r, 0, errors);
        errors.ThrowIfAny();

        var g = new PermissionGroup { GroupName = name, Description = r.Description?.Trim() ?? "" };
        uow.PermissionGroups.Add(g);
        await uow.SaveChangesAsync();
        return new GroupRow(g.PermissionGroupId, g.GroupName, g.Description, 0);
    }

    // Null = group not found.
    public async Task<GroupRow?> UpdateGroupAsync(int id, GroupEditRequest r)
    {
        var g = await uow.PermissionGroups.GetByIdAsync(id);
        if (g is null) return null;

        var errors = new ErrorBag();
        var name = await ValidateGroupAsync(r, id, errors);
        errors.ThrowIfAny();

        g.GroupName = name;
        g.Description = r.Description?.Trim() ?? "";
        await uow.SaveChangesAsync();
        Invalidate();   // group names show on the Roles screen
        return (await uow.PermissionGroups.GetRowsAsync()).First(x => x.Id == id);
    }

    private async Task<string> ValidateGroupAsync(GroupEditRequest r, int exceptId, ErrorBag e)
    {
        var name = e.Required("name", "Group name", r.Name, PermissionFieldLimits.MaxGroupNameLength);
        if (name.Length > 0 && !e.Has("name") && await uow.PermissionGroups.NameExistsAsync(name, exceptId))
            e.Add("name", "A group with this name already exists.");
        e.Optional("description", "Description", r.Description, PermissionFieldLimits.MaxGroupDescriptionLength);
        return name;
    }
}
