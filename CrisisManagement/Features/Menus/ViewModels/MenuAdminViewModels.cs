namespace CrisisManagement.Features.Menus.ViewModels;

// Menus administration screen (/admin/menus). Not to be confused with MenuNode, the tree a signed-in user sees in the navbar.

public sealed record MenuListItem(int Id, string Name, string Icon, string Url, int? ParentId, int DisplaySequence, bool IsAlwaysEnabled, bool IsEnabled);

public sealed class MenuDetail
{
    public int Id { get; set; }
    public string RowVersion { get; set; } = "";
    public string Name { get; set; } = "";
    public string Icon { get; set; } = "";
    public string Description { get; set; } = "";
    public string Url { get; set; } = "";
    public string DetailUrl { get; set; } = "";
    public string TemplateUrl { get; set; } = "";
    public string DetailTemplateUrl { get; set; } = "";
    public string ApiUrl { get; set; } = "";
    public string Comment { get; set; } = "";
    public int? ParentId { get; set; }
    public int DisplaySequence { get; set; }
    public bool IsAlwaysEnabled { get; set; }
    public bool IsEnabled { get; set; } = true;
}

// Create and update share one body (same fields as MenuDetail; the id comes from the route).
public sealed class MenuEditRequest
{
    public string? RowVersion { get; set; }
    public string? Name { get; set; }
    public string? Icon { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public string? DetailUrl { get; set; }
    public string? TemplateUrl { get; set; }
    public string? DetailTemplateUrl { get; set; }
    public string? ApiUrl { get; set; }
    public string? Comment { get; set; }
    public int? ParentId { get; set; }
    public int DisplaySequence { get; set; }
    public bool IsAlwaysEnabled { get; set; }
    public bool IsEnabled { get; set; } = true;
}

// A menu item that may be chosen as a parent, with its full path ("Administration > Maintenance").
public sealed record ParentOption(int Id, string Path);

public sealed record PermissionRow(int Id, int GroupId, string GroupName, string Name, string Value, string Description);

public sealed class PermissionEditRequest
{
    public int MenuId { get; set; }          // used on create; a permission cannot move to another menu
    public int GroupId { get; set; }
    public string? Name { get; set; }
    public string? Value { get; set; }
    public string? Description { get; set; }
}

public sealed record GroupRow(int Id, string Name, string Description, int PermissionCount);

public sealed class GroupEditRequest
{
    public string? Name { get; set; }
    public string? Description { get; set; }
}
