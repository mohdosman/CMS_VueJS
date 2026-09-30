namespace CMS.Data.Models;

// RBS_MenuItem / RBS_Permission: only the columns the menu and permission checks read.
public sealed class MenuItem
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Url { get; set; } = "";
    public int? ParentMenuItemId { get; set; }
    public byte DisplaySequence { get; set; }
    public bool IsAlwaysEnabled { get; set; }
    public bool IsEnabled { get; set; }

    public ICollection<Permission> Permissions { get; set; } = [];
}

public sealed class Permission
{
    public int PermissionId { get; set; }
    public string Value { get; set; } = "";
    public int MenuItemId { get; set; }
}
