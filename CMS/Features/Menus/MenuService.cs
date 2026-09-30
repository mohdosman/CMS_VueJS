using System.Security.Claims;
using CMS.Data;
using CMS.Data.Models.Domain;
using CMS.Data.Models.Identity;
using CMS.Shared.Constants;
using Microsoft.Extensions.Caching.Memory;

namespace CMS.Features.Menus;

public sealed record MenuNode(int Id, string Name, string Icon, string Url, List<MenuNode> Children);

// Builds the menu a user may see from their permission claims (the same rules as the
// Blazor CMS MenuService): an item shows if it is always-enabled or the user holds one
// of its permissions; parents of visible items show too; Administrators see everything.
public sealed class MenuService(IUnitOfWork uow, IMemoryCache cache)
{
    // RBS_MenuItem.Icon holds MudBlazor icon ids; the SPA uses Font Awesome 4.
    private static readonly Dictionary<string, string> IconMap = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Home"] = "fa-home", ["Description"] = "fa-file-text-o", ["MedicalServices"] = "fa-medkit",
        ["Emergency"] = "fa-ambulance", ["Assessment"] = "fa-bar-chart", ["AdminPanelSettings"] = "fa-cogs",
        ["ChangePassword"] = "fa-lock", ["Security"] = "fa-shield", ["SettingsApplications"] = "fa-cog"
    };

    public async Task<List<MenuNode>> GetMenuAsync(ClaimsPrincipal user)
    {
        var rows = await cache.GetOrCreateAsync("menu:rows", async e =>
        {
            e.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            return await uow.MenuItems.GetEnabledWithPermissionsAsync();
        }) ?? [];

        // "Logout" is the header's own button.
        rows = rows.Where(m => !string.Equals(m.Url, "/logout", StringComparison.OrdinalIgnoreCase)).ToList();

        var granted = user.FindAll(AppClaimTypes.Permission).Select(c => c.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var v in granted.Where(v => v.EndsWith(".edit", StringComparison.OrdinalIgnoreCase)).ToList())
            granted.Add(v[..^5] + ".view");

        var byId = rows.ToDictionary(m => m.MenuItemId);
        var allowed = user.IsInRole(AppRoles.Admin)
            ? byId.Keys.ToHashSet()
            : rows.Where(m => m.IsAlwaysEnabled || m.Permissions.Any(p => granted.Contains(p.Value))).Select(m => m.MenuItemId).ToHashSet();

        foreach (var id in allowed.ToList())
        {
            var parent = byId[id].ParentMenuItemId;
            while (parent is int pid && byId.ContainsKey(pid) && allowed.Add(pid))
                parent = byId[pid].ParentMenuItemId;
        }

        var byParent = rows.Where(m => allowed.Contains(m.MenuItemId)).ToLookup(m => m.ParentMenuItemId);
        return Build(byParent, null);
    }

    public static IEnumerable<MenuNode> Flatten(IEnumerable<MenuNode> nodes) =>
        nodes.SelectMany(n => new[] { n }.Concat(Flatten(n.Children)));

    private static List<MenuNode> Build(ILookup<int?, MenuItem> byParent, int? parentId) =>
        byParent[parentId].OrderBy(m => m.DisplaySequence).Select(m => new MenuNode(
            m.MenuItemId, m.MenuItemName, ToFontAwesome(m.Icon), NormalizeUrl(m.Url), Build(byParent, m.MenuItemId))).ToList();

    private static string ToFontAwesome(string icon) =>
        IconMap.TryGetValue(icon.Split('.').Last(), out var fa) ? fa : "";

    // Some rows omit the leading slash ("services/servicefileupload").
    private static string NormalizeUrl(string url) =>
        string.IsNullOrWhiteSpace(url) ? "" : "/" + url.Trim().TrimStart('/');
}
