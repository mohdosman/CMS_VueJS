using System.Text.RegularExpressions;

namespace CMS.Shared.Constants;

/// <summary>
/// Per-role scope permissions ("users.role.&lt;slug&gt;") control which roles a non-admin can see and
/// assign on the user screens. Each role gets one when it is created.
/// </summary>
public static partial class UserRolePermissionConstants
{
    public const string PermissionPrefix = "users.role";
    public const string MenuItemName = "Users";
    public const string PermissionGroupName = "Users Permissions";

    // Same slug rule as the Blazor CMS RemoveSpecialCharacters().
    public static string Slug(string s) => NonSlug().Replace(s, "").ToLowerInvariant();
    public static string PermissionValue(string roleName) => $"{PermissionPrefix}.{Slug(roleName)}";

    [GeneratedRegex("[^a-zA-Z0-9_.]+")] private static partial Regex NonSlug();
}
