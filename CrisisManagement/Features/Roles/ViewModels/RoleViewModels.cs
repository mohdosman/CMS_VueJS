namespace CrisisManagement.Features.Roles.ViewModels;

public sealed class RoleSearchRequest
{
    public string? Name { get; set; }
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDesc { get; set; }
}

public sealed record RoleListItem(int Id, string Name);

public sealed class RoleDetail
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string RowVersion { get; set; } = "";
    public List<string> Permissions { get; set; } = [];
}

// Create and update share one body: the name and the full set of permission values the role should hold.
public sealed class RoleEditRequest
{
    public string? Name { get; set; }
    public string? RowVersion { get; set; }
    public List<string> Permissions { get; set; } = [];
}

public sealed record PermissionItem(string Name, string Value, string Description);
public sealed record PermissionGroupItem(string GroupName, List<PermissionItem> Items);
