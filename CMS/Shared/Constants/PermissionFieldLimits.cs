namespace CMS.Shared.Constants;

/// <summary>
/// Column lengths for permissions and permission groups, verified against INFORMATION_SCHEMA
/// rather than the EF mapping, which has been wrong elsewhere in this codebase.
/// </summary>
public static class PermissionFieldLimits
{
    /// <summary>RBS_Permission.Name / Value / Description, all varchar(250).</summary>
    public const int MaxPermissionTextLength = 250;

    /// <summary>RBS_PermissionGroup.GroupName, varchar(100).</summary>
    public const int MaxGroupNameLength = 100;

    /// <summary>RBS_PermissionGroup.Description, varchar(250).</summary>
    public const int MaxGroupDescriptionLength = 250;
}
